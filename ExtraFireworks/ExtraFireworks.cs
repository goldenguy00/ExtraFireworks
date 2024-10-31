using System.Collections.Generic;
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
        internal static List<ItemBase> items = [];

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            PluginConfig.Init(Config);
            fireworkLauncherPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/FireworkLauncher");
            fireworkPrefab = fireworkLauncherPrefab.GetComponent<FireworkLauncher>().projectilePrefab;
            
            //Define all the items
            new FireworkAbility();
            new FireworkDaisy();
            new FireworkDrones();
            new FireworkMushroom();
            new FireworkOnHit();
            new FireworkOnKill();
            new FireworkGrandFinale();
            new PowerWorksVoid();

            // Load assetpack and initialize
            var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "extrafireworks"));
            foreach (var item in items)
            {
                item.Init(bundle);
            }
            
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        public static FireworkLauncher FireFireworks(CharacterBody owner, int count)
        {
            if (!owner)
                return null;

            var transform = owner.coreTransform ? owner.coreTransform : owner.transform;
            var fireworkLauncher = SpawnFireworks(transform, owner, count);
            fireworkLauncher.transform.parent = transform;

            return fireworkLauncher;
        }

        // Firework item formula: 4 + 4 * stack
        public static FireworkLauncher SpawnFireworks(Transform target, CharacterBody owner, int count, bool attach = true)
        {
            if (!owner || !target)
                return null;

            var position = target.position + (Vector3.up * 2f);
            if (target.TryGetComponent<ModelLocator>(out var locator) && locator.modelTransform && locator.modelTransform.TryGetComponent<ChildLocator>(out var loc))
            {
                var located = loc.FindChild("FireworkOrigin");
                if (located)
                    position = located.position;
            }

            if (target.TryGetComponent<CharacterBody>(out var body))
                position += Vector3.up * body.radius;
            
            var fireworkLauncher = CreateLauncher(owner, position, count);
            if (attach)
                fireworkLauncher.transform.parent = target;
            
            return fireworkLauncher;
        }

        public static FireworkLauncher CreateLauncher(CharacterBody owner, Vector3 position, int count)
        {
            if (!owner)
                return null;

            var fireworkLauncher = Instantiate(fireworkLauncherPrefab, position, Quaternion.identity).GetComponent<FireworkLauncher>();

            fireworkLauncher.owner = owner.gameObject;
            fireworkLauncher.team = owner.teamComponent.teamIndex;
            fireworkLauncher.crit = Util.CheckRoll(owner.crit, owner.master);
            fireworkLauncher.remaining = count;

            return fireworkLauncher;
        }
    }
}
