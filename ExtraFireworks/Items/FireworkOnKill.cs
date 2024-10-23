using ExtraFireworks.Config;
using RoR2;

namespace ExtraFireworks.Items
{
    public class FireworkOnKill : BaseFireworkItem<FireworkOnKill>
    {
        private readonly ConfigurableLinearScaling scaler;

        public FireworkOnKill() : base()
        {
            scaler = new ConfigurableLinearScaling("", GetConfigSection(), 2, 1);
        }

        public override string GetName() => "FireworkOnKill";

        public override string GetPickupModelName() => "Will-o-the-Firework.prefab";

        public override float GetModelScale() => 1.1f;

        public override string GetPickupIconName() => "BottledFireworks.png";

        public override ItemTier GetTier() => ItemTier.Tier2;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string GetItemName() => "Will-o'-the-Firework";

        public override string GetItemPickup() => "Spawn fireworks on kill";

        public override string GetItemDescription()
        {
            return $"On <style=cIsDamage>killing an enemy</style>, release a " +
                   $"<style=cIsDamage>barrage of {scaler.Base}</style> " +
                   $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style> for " +
                   $"<style=cIsDamage>300%</style> base damage each.";
        }

        public override string GetItemLore() => "Revolutionary design.";

        public override void AddHooks()
        {
            // Implement fireworks on kill
            GlobalEventManager.onCharacterDeathGlobal += this.GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (!report.attacker || !report.attackerBody)
                return;

            var attackerCharacterBody = report.attackerBody;
            if (attackerCharacterBody.inventory)
            {
                var count = attackerCharacterBody.inventory.GetItemCount(Item);

                if (count <= 0)
                    return;

                if (!report.victim)
                    return;

                var victimBody = report.victim.body;
                if (!victimBody)
                    return;

                var trans = victimBody.coreTransform ? victimBody.coreTransform : victimBody.transform;
                ExtraFireworks.SpawnFireworks(trans, attackerCharacterBody, scaler.GetValueInt(count), false);
            }
        }
    }
}