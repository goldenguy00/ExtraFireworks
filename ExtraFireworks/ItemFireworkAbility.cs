using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace ExtraFireworks;

public class ItemFireworkAbility : FireworkItem
{
    private ConfigurableLinearScaling scaler;
    private ConfigEntry<bool> noSkillRestriction;
    
    public ItemFireworkAbility(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        scaler = new ConfigurableLinearScaling(config, "", GetConfigSection(), 1, 1);
        noSkillRestriction = config.Bind(GetConfigSection(), "PrimaryAbilityFireworks", false,
            "Whether abilities without a cooldown should spawn fireworks... be wary of brokenness, especially on Commando and Railgunner");
    }

    public override string GetName()
    {
        return "FireworkAbility";
    }

    public override string GetPickupModelName()
    {
        return "Firework-Stuffed Head.prefab";
    }

    public override float GetModelScale()
    {
        return 1.1f;
    }

    public override string GetPickupIconName()
    {
        return "FireworkStuffedHead.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.Tier2;
    }

    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.AIBlacklist };
    }
    
    public override string GetItemName()
    {
        return "Firework-Stuffed Head";
    }

    public override string GetItemPickup()
    {
        return "Using abilities now spawns fireworks";
    }

    public override string GetItemDescription()
    {
        return $"Using a <style=cIsUtility>non-primary skill</style> fires <style=cIsDamage>{scaler.Base}</style> " +
               $"<style=cStack>(+{scaler.Scaling} per stack)</style> for <style=cIsDamage>300% base damage</style>.";
    }

    public override string GetItemLore()
    {
        return "Holy shit it's a head with fireworks sticking out of it";
    }

    public override void AddHooks()
    {
        On.RoR2.CharacterBody.OnSkillActivated += (orig, self, skill) =>
        {
            if (!self.inventory || skill == null)
                goto end;

            var stack = self.inventory.GetItemCount(Item);
            if (stack > 0)
            {
                if (noSkillRestriction.Value || skill.baseRechargeInterval >= 1f - Mathf.Epsilon 
                    && skill.skillDef && skill.skillDef.stockToConsume > 0)
                    ExtraFireworks.FireFireworks(self, scaler.GetValueInt(stack));
            }

            end:
            orig(self, skill);
        };
    }
}