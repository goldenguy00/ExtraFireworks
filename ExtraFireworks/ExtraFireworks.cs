using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using ExtraFireworks.Config;
using ExtraFireworks.Items;
using RoR2;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ExtraFireworks
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.RumblingJOSEPH.VoidItemAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ExtraFireworks : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "PhysicsFox";
        public const string PluginName = "ExtraFireworks";
        public const string PluginVersion = "1.5.4";

        public static GameObject fireworkLauncherPrefab;
        public static GameObject fireworkPrefab;
        internal static List<FireworkItem> items = [];

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            PluginConfig.Init(Config);
            fireworkLauncherPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/FireworkLauncher");
            fireworkPrefab = fireworkLauncherPrefab.GetComponent<FireworkLauncher>().projectilePrefab;
            
            //Define all the items
            new ItemFireworkAbility();
            new ItemFireworkDaisy();
            new ItemFireworkDrones();
            new ItemFireworkMushroom();
            new ItemFireworkOnHit();
            new ItemFireworkOnKill();
            new ItemFireworkFinale();
            new ItemFireworkVoid();

            // Load assetpack and initialize
            var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "extrafireworks"));
            foreach (var item in items)
            {
                item.Init(bundle);
            }

            // Bypass 3D model scaling
            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
            
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            orig(self, modelObjectOverride);
            if (self.pickupIndex == PickupIndex.none || self.pickupIndex.pickupDef == null)
                return;

            // fix later
            if ((self.modelObject && self.modelObject.name == "PickupMystery(Clone)") || // edge case where item in trishop
                (self.highlight && self.highlight.name == "CommandCube(Clone)")) // edge case where item turns into command essence
            {
                return;
            }

            foreach (var item in items)
            {
                if (item.IsEnabled() && self.pickupIndex.pickupDef.itemTier == item.Item.tier
                    && self.pickupIndex.pickupDef.itemIndex == item.Item.itemIndex)
                {
                    self.modelObject.transform.localScale *= item.GetModelScale();
                    break;
                }
            }
        }

        public static FireworkLauncher FireFireworks(CharacterBody owner, int count)
        {
            var fireworkLauncher = SpawnFireworks(owner.coreTransform, owner, count);
            fireworkLauncher.transform.parent = owner.coreTransform;
            return fireworkLauncher;
        }

        // Firework item formula: 4 + 4 * stack
        public static FireworkLauncher SpawnFireworks(Transform target, CharacterBody owner, int count, bool attach = true)
        {
            Transform located = null;
            if (target.TryGetComponent<ModelLocator>(out var locator) && locator.modelTransform)
            {
                var chLoc = locator.modelTransform.GetComponent<ChildLocator>();
                if (chLoc)
                    located = chLoc.FindChild("FireworkOrigin");
            }

            var position = located ? located.position : (target.position + Vector3.up * 2f);
            
            if (target.TryGetComponent<CharacterBody>(out var body))
                position += Vector3.up * body.radius;
            
            var fireworkLauncher = CreateLauncher(owner, position, count);
            if (attach)
                fireworkLauncher.transform.parent = target;
            
            return fireworkLauncher;
        }

        public static FireworkLauncher CreateLauncher(CharacterBody owner, Vector3 position, int count)
        {
            var fireworkLauncher = Instantiate(fireworkLauncherPrefab, position, Quaternion.identity).GetComponent<FireworkLauncher>();

            fireworkLauncher.owner = owner?.gameObject;
            if (owner)
            {
                var tc = owner.teamComponent;
                fireworkLauncher.team = tc ? tc.teamIndex : TeamIndex.None;
                fireworkLauncher.crit = Util.CheckRoll(owner.crit, owner.master);
            }

            fireworkLauncher.remaining = count;
            return fireworkLauncher;
        }
    }
}
