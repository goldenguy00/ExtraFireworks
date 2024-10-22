using System;
using System.Linq;
using BepInEx.Configuration;
using ExtraFireworks.Config;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace ExtraFireworks.Items
{
    public abstract class FireworkItem<T> : FireworkItem where T : FireworkItem<T>
    {
        public static T Instance { get; private set; }

        public FireworkItem()
        {
            if (Instance != null)
                throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");

            Instance = this as T;
            ExtraFireworks.items.Add(this);
        }
    }

    public abstract class FireworkItem
    {
        public ItemDef Item { get; protected set; }

        protected ConfigEntry<bool> itemEnabled;

        public abstract string GetName();
        public abstract string GetPickupModelName();
        public abstract string GetPickupIconName();
        public abstract ItemTier GetTier();
        public abstract ItemTag[] GetTags();
        public abstract string GetItemName();
        public abstract string GetItemPickup();
        public abstract string GetItemDescription();
        public abstract string GetItemLore();
        public abstract void AddHooks();

        public virtual ItemDisplayRuleDict GetDisplayRules() => new(null);

        public string GetPickupModel() => $"Assets/ImportModels/{GetPickupModelName()}";

        public virtual float GetModelScale() => 1.0f;

        public string GetPickupIcon() => $"Assets/Import/{GetPickupIconName()}";

        public virtual string GetConfigSection() => GetName();

        public bool IsEnabled() => itemEnabled?.Value ?? true;

        public virtual void Init(AssetBundle bundle)
        {
            if (GetTier() != ItemTier.NoTier)
                itemEnabled = PluginConfig.config.Bind(GetConfigSection(), "Enabled", true, "Item enabled?");

            Item = ScriptableObject.CreateInstance<ItemDef>();

            var subtoken = GetName().ToUpper();
            Item.name = $"ITEM_{subtoken}_NAME";
            Item.nameToken = $"ITEM_{subtoken}_NAME";
            Item.pickupToken = $"ITEM_{subtoken}_PICKUP";
            Item.descriptionToken = $"ITEM_{subtoken}_DESC";

            // No lore for consumed item
            if (GetTier() != ItemTier.NoTier)
                Item.loreToken = $"ITEM_{subtoken}_LORE";

            Item.deprecatedTier = GetTier();
            // STINKY!!!
            if (GetTier() == ItemTier.VoidTier1)
                Item.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME"); //Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            Item.canRemove = GetTier() != ItemTier.NoTier;
            Item.hidden = false;
            Item.tags = GetTags();

            if (bundle != null)
            {
                Item.pickupModelPrefab = bundle.LoadAsset<GameObject>(GetPickupModel());
                Item.pickupIconSprite = bundle.LoadAsset<Sprite>(GetPickupIcon());
            }

            if (IsEnabled())
            {
                LanguageAPI.Add(Item.nameToken, GetItemName());
                LanguageAPI.Add(Item.pickupToken, GetItemPickup());
                LanguageAPI.Add(Item.descriptionToken, GetItemDescription());
                // No lore for consumed item
                if (GetTier() != ItemTier.NoTier)
                    LanguageAPI.Add(Item.loreToken, GetItemLore());

                ItemAPI.Add(new CustomItem(Item, GetDisplayRules()));

                AddHooks();
            }
        }
    }
}