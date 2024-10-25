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
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T Instance { get; private set; }

        public ItemBase()
        {
            if (Instance != null)
                throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");

            Instance = this as T;
            ExtraFireworks.items.Add(this);
        }
    }

    /// <summary>
    /// Dont inherit this class directly, use FireworkItem<T> instead!!
    /// </summary>
    public abstract class ItemBase
    {
        public ItemDef Item { get; protected set; }

        protected ConfigEntry<bool> itemEnabled;
        public bool IsEnabled => itemEnabled?.Value ?? true;

        public abstract string UniqueName { get; }
        public abstract string PickupModelName { get; }
        public abstract string PickupIconName { get; }
        public abstract ItemTier Tier { get; }
        public abstract ItemTag[] Tags { get; }
        public abstract string ItemName { get; }
        public abstract string ItemPickupDescription { get; }
        public abstract string ItemDescription { get; }
        public abstract string ItemLore { get; }

        public virtual ItemDisplayRuleDict DisplayRules { get; }
        public virtual float ModelScale { get; } = 1.0f;
        public virtual string ConfigSection => UniqueName;
        public virtual bool RequireSotV => false;

        public abstract void AddHooks();

        public virtual void Init(AssetBundle bundle)
        {
            if (Tier != ItemTier.NoTier)
                itemEnabled = PluginConfig.config.BindOption(ConfigSection, "Enabled", true, "Item enabled?", restartRequired: true);

            if (IsEnabled)
            {
                var subtoken = UniqueName.ToUpper();

                var item = new CustomItem(
                    name: UniqueName,
                    nameToken: $"ITEM_{subtoken}_NAME",
                    descriptionToken: $"ITEM_{subtoken}_DESC",
                    loreToken: Tier == ItemTier.NoTier ? "" : $"ITEM_{subtoken}_LORE",
                    pickupToken: $"ITEM_{subtoken}_PICKUP",
                    pickupIconSprite: bundle.LoadAsset<Sprite>($"Assets/Import/{PickupIconName}"),
                    pickupModelPrefab: bundle.LoadAsset<GameObject>($"Assets/ImportModels/{PickupModelName}"),
                    tags: Tags,
                    tier: Tier,
                    canRemove: Tier != ItemTier.NoTier,
                    hidden: false,
                    unlockableDef: null,
                    itemDisplayRules: DisplayRules);

                if (RequireSotV)
                    item.ItemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME");
                this.Item = item.ItemDef;
                if (this.Item.pickupModelPrefab)
                    this.Item.pickupModelPrefab.transform.localScale *= this.ModelScale;

                LanguageAPI.Add(Item.nameToken, ItemName);
                LanguageAPI.Add(Item.pickupToken, ItemPickupDescription);
                LanguageAPI.Add(Item.descriptionToken, ItemDescription);
                // No lore for consumed item
                if (Tier != ItemTier.NoTier)
                    LanguageAPI.Add(Item.loreToken, ItemLore);

                ItemAPI.Add(item);

                AddHooks();
            }
        }
    }
}