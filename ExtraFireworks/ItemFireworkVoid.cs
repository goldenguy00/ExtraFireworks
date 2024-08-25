using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using VoidItemAPI;

namespace ExtraFireworks;

public class ItemFireworkVoid : FireworkItem
{
    private ConfigEntry<int> fireworksPerUse;
    private ConfigEntry<float> hpThreshold;
    private ConfigEntry<float> cooldown;
    
    private Dictionary<CharacterBody, float> cooldownTimers;
    
    private bool voidInitialized = false;

    public ItemFireworkVoid(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworksPerUse = config.Bind(GetConfigSection(), "FireworksPerUse", 15,
            "Number of fireworks per consumption");
        hpThreshold = config.Bind(GetConfigSection(), "HpThreshold", 0.25f,
            "HP threshold before Power Works is consumed");
        cooldown = config.Bind(GetConfigSection(), "Cooldown", 20f,
            "Cooldown before item can be consumed again");

        cooldownTimers = new Dictionary<CharacterBody, float>();
    }

    public override string GetName()
    {
        return "PowerWorks";
    }

    public override string GetPickupModelName()
    {
        return "Power Works.prefab";
    }
    
    public override float GetModelScale()
    {
        return 0.4f;
    }

    public override string GetPickupIconName()
    {
        return "PowerWorks.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.VoidTier1;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.AIBlacklist };
    }

    public override string GetItemName()
    {
        return "Power 'Works";
    }

    public override string GetItemPickup()
    {
        return "Release a barrage of fireworks at low health. Consumed on use.";
    }

    public override string GetItemDescription()
    {
        return $"Taking damage to below 25% health consumes this item (Refreshes next stage), releasing a barrage of fireworks dealing 15x300% (+15 Fireworks per stack) damage.";
    }

    public override string GetItemLore()
    {
        return "MMMM YUM.";
    }

    // TODO double-check that !NetworkServer.active needed -- means host manages all this
    public override void FixedUpdate()
    {
        if (!NetworkServer.active)
            return;
        
        foreach (var body in cooldownTimers.Keys)
            cooldownTimers[body] -= Time.fixedDeltaTime;
    }

    public override void AddHooks()
    {
        On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
        {
            orig(self, info);
            
            var body = self.body;
            if (!body || !body.inventory || !NetworkServer.active)
                return;

            // Check if HP threshold met
            if (!(self.health / self.fullHealth <= hpThreshold.Value)) 
                return;
            
            
            var count = body.inventory.GetItemCount(Item.itemIndex);
            if (count <= 0 || (cooldownTimers.ContainsKey(body) && cooldownTimers[body] > 0)) 
                return;
            
            ExtraFireworks.FireFireworks(body, fireworksPerUse.Value);
            cooldownTimers[body] = cooldown.Value;
        };

        // Prevent cooldown timers from leaking memory by clearing dictionary every stage
        On.RoR2.Stage.BeginServer += (orig, self) =>
        {
            orig(self);

            if (!NetworkServer.active)
                return;
            
            cooldownTimers.Clear();
        };

        On.RoR2.ItemCatalog.SetItemDefs += (orig, newItemDefs) =>
        {
            orig(newItemDefs);
            
            if (!voidInitialized)
            {
                VoidItemAPI.VoidTransformation.CreateTransformation(Item, DLC1Content.Items.HealingPotion);
                voidInitialized = true;
            }
        };
    }
}