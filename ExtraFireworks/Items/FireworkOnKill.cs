using ExtraFireworks.Config;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkOnKill : ItemBase<FireworkOnKill>
    {
        private readonly ConfigurableLinearScaling scaler;

        public FireworkOnKill() : base()
        {
            scaler = new ConfigurableLinearScaling(ConfigSection, 2, 2);
        }

        public override string UniqueName => "FireworkOnKill";

        public override string PickupModelName => "Will-o-the-Firework.prefab";

        public override string PickupIconName => "BottledFireworks.png";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Will-o'-the-Firework";

        public override string ItemPickupDescription => "Spawn fireworks on kill";

        public override string ItemDescription =>
            $"On <style=cIsDamage>killing an enemy</style>, release a " +
            $"<style=cIsDamage>barrage of {scaler.Base}</style> " +
            $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style> for " +
            $"<style=cIsDamage>300%</style> base damage each.";

        public override string ItemLore => "Revolutionary design.";

        public override void AddHooks()
        {
            // Implement fireworks on kill
            GlobalEventManager.onCharacterDeathGlobal += this.GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (!NetworkServer.active)
                return;

            if (report.victimBody && report.attackerBody && report.attackerBody.inventory)
            {
                var count = report.attackerBody.inventory.GetItemCount(Item);

                if (!(count > 0))
                    return;
                
                var transform = report.victimBody.coreTransform ? report.victimBody.coreTransform : report.victimBody.transform;
                ExtraFireworks.SpawnFireworks(transform, report.attackerBody, scaler.GetValueInt(count), false);
            }
        }
    }
}