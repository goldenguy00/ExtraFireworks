using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using VoidItemAPI;

namespace ExtraFireworks;

public class ItemFireworkVoidConsumed : FireworkItem
{
    private ItemFireworkVoid parent;
    private bool voidInitialized = false;

    public ItemFireworkVoidConsumed(ExtraFireworks plugin, ConfigFile config, ItemFireworkVoid parent) : base(plugin, config)
    {
        this.parent = parent;
    }

    public override string GetName()
    {
        return "PowerWorksConsumed";
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
        return "PowerWorksConsumed.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.NoTier;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy, 
            ItemTag.CannotDuplicate };
    }

    public override string GetItemName()
    {
        return "Power 'Works (Consumed)";
    }

    public override string GetItemPickup()
    {
        return parent.GetItemPickup();
    }

    public override string GetItemDescription()
    {
        return parent.GetItemDescription();
    }

    public override string GetItemLore()
    {
        return parent.GetItemLore();
    }

    public override void AddHooks()
    {
        On.RoR2.ItemCatalog.SetItemDefs += (orig, newItemDefs) =>
        {
            orig(newItemDefs);
            
            if (!voidInitialized)
            {
                VoidTransformation.CreateTransformation(Item, DLC1Content.Items.HealingPotionConsumed);
                voidInitialized = true;
            }
        };
    }
}