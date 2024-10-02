using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace ExtraFireworks
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(VoidItemAPI.VoidItemAPI.MODGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ExtraFireworks : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "PhysicsFox";
        public const string PluginName = "ExtraFireworks";
        public const string PluginVersion = "1.3.0";
        
        private static List<FireworkItem> items;

        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            //Define all the items
            items = new List<FireworkItem>
            {
                new ItemFireworkAbility(this, Config),
                new ItemFireworkDaisy(this, Config),
                new ItemFireworkDrones(this, Config),
                new ItemFireworkMushroom(this, Config),
                new ItemFireworkOnHit(this, Config),
                new ItemFireworkOnKill(this, Config),
                //new ItemFireworkFinale(this, Config),
                //new ItemFireworkVoid(this, Config)
            };

            // Load assetpack and initialize
            using (var stream = Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream("ExtraFireworks.extrafireworks"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                foreach (var item in items)
                    item.Init(bundle);
            }

            // Bypass 3D model scaling
            On.RoR2.PickupDisplay.RebuildModel += (orig, self, modelObjectOverride) =>
            {
                orig(self, modelObjectOverride);

                if (self.pickupIndex == null || self.pickupIndex.pickupDef == null 
                    || self.modelObject && self.modelObject.name == "PickupMystery(Clone)" // edge case where item in trishop
                    || self.highlight && self.highlight.name == "CommandCube(Clone)") // edge case where item turns into command essence
                    return;

                foreach (var item in items)
                    if (item.IsEnabled() && self.pickupIndex.pickupDef.itemTier == item.Item.tier
                        && self.pickupIndex.pickupDef.itemIndex == item.Item.itemIndex)
                    {
                        self.modelObject.transform.localScale *= item.GetModelScale();
                        break;
                    }
            };
            
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }
        
        public void OnEnable()
        {
            foreach (var item in items)
                if (item.IsEnabled())
                    item.OnEnable();
        }

        public void OnDisable()
        {
            foreach (var item in items)
                if (item.IsEnabled())
                    item.OnDisable();
        }

        private void FixedUpdate()
        {
            foreach (var item in items)
                if (item.IsEnabled())
                    item.FixedUpdate();
        }

        public static FireworkLauncher FireFireworks(CharacterBody owner, int count)
        {
            var fl = SpawnFireworks(owner.coreTransform, owner, count);
            fl.gameObject.transform.parent = owner.coreTransform;
            return fl;
        }

        // Firework item formula: 4 + 4 * stack
        public static FireworkLauncher SpawnFireworks(Transform target, CharacterBody owner, int count, bool attach = true)
        {
            ModelLocator locator = target.GetComponent<ModelLocator>();
            Transform located = locator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild("FireworkOrigin");
            Vector3 position = located ? located.position : (target.position + Vector3.up * 2f);
            
            var body = target.GetComponent<CharacterBody>();
            if (body)
                position += Vector3.up * body.radius;
            
            var fl = CreateLauncher(owner, position, count);
            if (attach)
                fl.gameObject.transform.parent = target;
            
            return fl;
        }

        public static FireworkLauncher CreateLauncher(CharacterBody owner, Vector3 position, int count)
        {
            FireworkLauncher fireworkLauncher = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/FireworkLauncher"), position, Quaternion.identity).GetComponent<FireworkLauncher>();
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
