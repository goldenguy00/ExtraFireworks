using System;
using BepInEx.Configuration;
using MiscFixes.Modules;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
                itemEnabled = ExtraFireworks.instance.Config.BindOption(ConfigSection, "Enabled", "Item enabled?", true, Extensions.ConfigFlags.RestartRequired);
                if (itemEnabled.Value)
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

        public virtual void Init(AssetBundle bundle)
        {
            var subtoken = UniqueName.ToUpper();

            var item = new CustomItem(
                name: UniqueName,
                nameToken: $"ITEM_{subtoken}_NAME",
                descriptionToken: $"ITEM_{subtoken}_DESC",
                loreToken: "",
                pickupToken: $"ITEM_{subtoken}_PICKUP",
                pickupIconSprite: bundle.LoadAsset<Sprite>($"Assets/Import/{PickupIconName}"),
                pickupModelPrefab: bundle.LoadAsset<GameObject>($"Assets/ImportModels/{PickupModelName}"),
                tags: Tags,
                tier: Tier,
                canRemove: false,
                hidden: false,
                unlockableDef: null,
                itemDisplayRules: DisplayRules);

            this.Item = item.ItemDef;
#pragma warning disable CS0618 // Type or member is obsolete
            this.Item.deprecatedTier = Tier;
#pragma warning restore CS0618 // Type or member is obsolete

            if (RequireSotV)
                this.Item.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>(RoR2_DLC1_Common.DLC1_asset).WaitForCompletion();

            AdjustPickupModel();

            LanguageAPI.Add(Item.nameToken, ItemName);
            LanguageAPI.Add(Item.pickupToken, ItemPickupDescription);
            LanguageAPI.Add(Item.descriptionToken, ItemDescription);
            // No lore for consumed item
            if (Tier != ItemTier.NoTier)
            {
                Item.loreToken = $"ITEM_{subtoken}_LORE";
                Item.canRemove = true;
                Item.tags = [.. Item.tags, ItemTag.CanBeTemporary];

                LanguageAPI.Add(Item.loreToken, ItemLore);
            }

            ItemAPI.Add(item);

            AddHooks();
        }

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

        public virtual void AddHooks() { }

    }
}