﻿using BepInEx.Configuration;
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
        return "FireworkStuffedHead.prefab";
    }

    public override string GetPickupIconName()
    {
        return "FireworkStuffedHead.png";
    }

    public override ItemTiers GetTier()
    {
        return ItemTiers.Green;
    }

    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage };
    }

    public override float GetModelScale()
    {
        return .4f;
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
        return $"Whenever you use an <style=cIsUtility>ability with cooldown</style>, fire <style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style>.";
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