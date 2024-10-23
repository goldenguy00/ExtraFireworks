using System.Linq;
using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDaisy : BaseFireworkItem<FireworkDaisy>
    {
        internal static ConfigEntry<int> fireworksPerWave;

        public FireworkDaisy() : base()
        {
            fireworksPerWave = PluginConfig.config.Bind(GetConfigSection(), "FireworksPerWave", 40,
                "Number of fireworks per firework daisy wave");
        }

        public override string GetName() => "FireworkDaisy";

        public override string GetPickupModelName() => "Firework Daisy.prefab";

        public override float GetModelScale() => 1.5f;

        public override string GetPickupIconName() => "FireworkDaisy.png";

        public override ItemTier GetTier() => ItemTier.Tier2;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.HoldoutZoneRelated, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string GetItemName() => "Firework Daisy";

        public override string GetItemPickup() => "Periodically releases waves of fireworks during the teleporter event";

        public override string GetItemDescription()
        {
            return $"<style=cIsDamage>Releases a barrage of fireworks</style> during the " +
                   $"<style=cIsUtility>Teleporter event</style>, dealing " +
                   $"<style=cIsDamage>{fireworksPerWave.Value}x300%</style> base damage. " +
                   $"Occurs <style=cIsDamage>2</style> <style=cStack>(+1 per stack)</style> " +
                   $"<style=cIsDamage>times</style>.";
        }

        public override string GetItemLore() => "A lepton daisy with a firework jammed in it.";

        public override void AddHooks()
        {
            On.RoR2.HoldoutZoneController.Awake += this.HoldoutZoneController_Awake;
        }

        private void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig(self);

            self.gameObject.AddComponent<FireworkDaisyBehaviour>();
        }
    }

    public class FireworkDaisyBehaviour : MonoBehaviour
    {
        public float minSecondsBetweenPulses = 1f;

        private HoldoutZoneController holdoutZone;
        
        private float previousPulseFraction;

        private int pulseCount;

        private float secondsUntilPulseAvailable;

        private void OnEnable()
        {
            this.holdoutZone = this.GetComponent<HoldoutZoneController>();
            this.previousPulseFraction = this.holdoutZone.charge;
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!this.holdoutZone || this.holdoutZone.charge >= 1f)
            {
                Destroy(this);
                return;
            }

            if (this.secondsUntilPulseAvailable > 0f)
            {
                this.secondsUntilPulseAvailable -= Time.fixedDeltaTime;
                return;
            }

            this.pulseCount = Util.GetItemCountForTeam(TeamIndex.Player, FireworkDaisy.Instance.Item.itemIndex, false);
            float nextFraction = CalculateNextPulseFraction();

            if (nextFraction < this.holdoutZone.charge)
            {
                this.Pulse();
                this.previousPulseFraction = nextFraction;
                this.secondsUntilPulseAvailable = this.minSecondsBetweenPulses;
            }
        }

        private float CalculateNextPulseFraction()
        {
            float num = 1f / (pulseCount + 1f);
            for (int i = 1; i <= pulseCount; i++)
            {
                float num2 = i * num;
                if (num2 > previousPulseFraction)
                {
                    return num2;
                }
            }
            return 1f;
        }

        private void Pulse()
        {
            var bodies = TeamComponent.GetTeamMembers(TeamIndex.Player).Select(tc => tc.body).Where(body => body && body.inventory && body.inventory.GetItemCount(FireworkDaisy.Instance.Item) > 0);
            if (bodies.Any())
            {
                ExtraFireworks.SpawnFireworks(this.transform, bodies.ElementAt(Random.Range(0, bodies.Count())), FireworkDaisy.fireworksPerWave.Value);
            }
        }
    }
}