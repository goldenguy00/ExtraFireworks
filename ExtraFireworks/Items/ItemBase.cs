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

            if (Tier != ItemTier.NoTier)
            {
                itemEnabled = PluginConfig.BindOption(ConfigSection, "Enabled", true, "Item enabled?", restartRequired: true);
                ExtraFireworks.items.Add(this);
            }
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

        public abstract string ItemName { get; }
        public abstract string UniqueName { get; }
        public abstract string PickupModelName { get; }
        public abstract string PickupIconName { get; }
        public abstract ItemTier Tier { get; }
        public abstract ItemTag[] Tags { get; }
        public abstract string ItemPickupDescription { get; }
        public abstract string ItemDescription { get; }
        public abstract string ItemLore { get; }

        public virtual ItemDisplayRuleDict DisplayRules { get; }
        public virtual Vector3? ModelScale { get; }
        public virtual string ConfigSection => UniqueName;
        public virtual bool RequireSotV => false;

        public abstract void AddHooks();
        
        public virtual void AdjustPickupModel()
        {
            var prefab = this.Item?.pickupModelPrefab;
            if (prefab)
            {
                ExtraFireworks.ConvertAllRenderersToHopooShader(prefab);

                if (this.ModelScale.HasValue)
                    prefab.transform.localScale = this.ModelScale.Value;

                if (!prefab.TryGetComponent<ModelPanelParameters>(out var mdlParams))
                    mdlParams = prefab.AddComponent<ModelPanelParameters>();

                if (!mdlParams.focusPointTransform)
                {
                    mdlParams.focusPointTransform = new GameObject("FocusPoint").transform;
                    mdlParams.focusPointTransform.SetParent(this.Item.pickupModelPrefab.transform);
                }
                if (!mdlParams.cameraPositionTransform)
                {
                    mdlParams.cameraPositionTransform = new GameObject("CameraPosition").transform;
                    mdlParams.cameraPositionTransform.SetParent(this.Item.pickupModelPrefab.transform);
                }
            }
        }

        public virtual void Init(AssetBundle bundle)
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

            this.Item = item.ItemDef;
            this.Item.deprecatedTier = Tier;

            if (RequireSotV)
                this.Item.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME");

            AdjustPickupModel();

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