using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace ExtraFireworks;

public class ItemFireworkAbility : FireworkItem
{
    private ConfigurableLinearScaling scaler;
    private ConfigEntry<bool> primaryFireworks;
    
    public ItemFireworkAbility(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        scaler = new ConfigurableLinearScaling(config, "", GetConfigSection(), 1, 1);
        primaryFireworks = config.Bind(GetConfigSection(), "PrimaryAbilityFireworks", false,
            "Whether primary should spawn fireworks or not... very unbalanced on commando");
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
        return $"Whenever you use an <style=cIsUtility>non-primary ability</style>, fire <style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style>.";
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
                if (primaryFireworks.Value || self.skillLocator != null && skill != self.skillLocator.primary)
                    ExtraFireworks.FireFireworks(self, scaler.GetValueInt(stack));
            }

            end:
            orig(self, skill);
        };
    }
}