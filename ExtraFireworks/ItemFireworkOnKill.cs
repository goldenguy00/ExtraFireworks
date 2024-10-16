using BepInEx.Configuration;
using RoR2;

namespace ExtraFireworks;

public class ItemFireworkOnKill : FireworkItem
{
    private ConfigurableLinearScaling scaler;
    
    public ItemFireworkOnKill(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        scaler = new ConfigurableLinearScaling(config, "", GetConfigSection(), 2, 1);
    }

    public override string GetName()
    {
        return "FireworkOnKill";
    }

    public override string GetPickupModelName()
    {
        return "Will-o-the-Firework.prefab";
    }
    
    public override float GetModelScale()
    {
        return 1.1f;
    }

    public override string GetPickupIconName()
    {
        return "BottledFireworks.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.Tier2;
    }

    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };
    }
    
    public override string GetItemName()
    {
        return "Will-o'-the-Firework";
    }

    public override string GetItemPickup()
    {
        return "Spawn fireworks on kill";
    }

    public override string GetItemDescription()
    {
        return $"On <style=cIsDamage>killing an enemy</style>, release a " +
               $"<style=cIsDamage>barrage of {scaler.Base}</style> " +
               $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style> for " +
               $"<style=cIsDamage>300%</style> base damage each.";
    }

    public override string GetItemLore()
    {
        return "Revolutionary design.";
    }

    public override void AddHooks()
    {
        // Implement fireworks on kill
        GlobalEventManager.onCharacterDeathGlobal += (report) =>
        {
            if (!report.attacker || !report.attackerBody)
                return;

            var attackerCharacterBody = report.attackerBody;
            if (attackerCharacterBody.inventory )
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
        };
    }
}