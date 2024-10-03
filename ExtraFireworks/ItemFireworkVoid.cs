using System;
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
    public ConfigEntry<int> fireworksPerStack;
    public ConfigEntry<float> hpThreshold;

    public ItemFireworkVoidConsumed ConsumedItem;
    private bool voidInitialized = false;

    public ItemFireworkVoid(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworksPerStack = config.Bind(GetConfigSection(), "FireworksPerUse", 30,
            "Number of fireworks per consumption");
        hpThreshold = config.Bind(GetConfigSection(), "HpThreshold", 0.25f,
            "HP threshold before Power Works is consumed");
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
        return "Release a barrage of fireworks at low health. Consumed on use. Refreshes every stage.";
    }

    public override string GetItemDescription()
    {
        var displayThreshold = (int)(Mathf.Round(hpThreshold.Value * 100) + Mathf.Epsilon);
        return $"Taking damage to below {displayThreshold}% health consumes this item (Refreshes next stage), " +
               $"releasing a barrage of fireworks dealing {fireworksPerStack.Value}x300% (+{fireworksPerStack.Value} " +
               $"Fireworks per stack) damage.";
    }

    public override string GetItemLore()
    {
        return "MMMM YUM.";
    }

    public override void AddHooks()
    {
        On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
        {
            orig(self, info);
            
            var body = self.body;
            if (!body || !body.inventory || !body.master || !NetworkServer.active)
                return;

            // Check if HP threshold met
            if (!(self.health / self.fullHealth <= hpThreshold.Value)) 
                return;
            
            
            var count = body.inventory.GetItemCount(Item.itemIndex);
            if (count <= 0) 
                return;
            
            body.inventory.RemoveItem(Item, count);
            body.inventory.GiveItem(ConsumedItem.Item, count);
            CharacterMasterNotificationQueue.SendTransformNotification(body.master, Item.itemIndex, 
                ConsumedItem.Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            
            ExtraFireworks.FireFireworks(body, fireworksPerStack.Value * count);
        };
        
        On.RoR2.ItemCatalog.SetItemDefs += (orig, newItemDefs) =>
        {
            orig(newItemDefs);
            
            if (!voidInitialized)
            {
                VoidTransformation.CreateTransformation(Item, DLC1Content.Items.HealingPotion);
                voidInitialized = true;
            }
        };

        On.RoR2.CharacterMaster.OnServerStageBegin += (orig, self, stage) =>
        {
            orig(self, stage);

            var consumedCount = self.inventory.GetItemCount(ConsumedItem.Item);
            if (!self.inventory || consumedCount <= 0)
                return;

            self.inventory.RemoveItem(ConsumedItem.Item, consumedCount);
            self.inventory.GiveItem(Item, consumedCount);
            CharacterMasterNotificationQueue.SendTransformNotification(self, ConsumedItem.Item.itemIndex, 
                Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
        };
    }
}