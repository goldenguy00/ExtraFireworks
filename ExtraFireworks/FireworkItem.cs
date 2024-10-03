using System.Linq;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ExtraFireworks;

public abstract class FireworkItem
{
    public ItemDef Item { get; protected set; }

    protected ExtraFireworks plugin;
    protected ConfigFile config;
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
    
    public virtual void AddHooks() { }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void FixedUpdate() { }
    
    public virtual ItemDisplayRuleDict GetDisplayRules()
    {
        return new ItemDisplayRuleDict(null);
    }

    public string GetPickupModel()
    {
        return $"Assets/ImportModels/{GetPickupModelName()}";
    }

    public virtual float GetModelScale()
    {
        return 1.0f;
    }
    
    public string GetPickupIcon()
    {
        return $"Assets/Import/{GetPickupIconName()}";
    }

    public virtual string GetConfigSection()
    {
        return GetName();
    }
    
    public bool IsEnabled()
    {
        return itemEnabled.Value;
    }
    
    protected FireworkItem(ExtraFireworks plugin, ConfigFile config)
    {
        this.plugin = plugin;
        this.config = config;
    }

    public virtual void Init(AssetBundle bundle)
    {
        if (GetTier() != ItemTier.NoTier)
            itemEnabled = config.Bind(GetConfigSection(), "Enabled", true, "Item enabled?");
        
        Item = ScriptableObject.CreateInstance<ItemDef>();
        
        var subtoken = GetName().ToUpper();
        Item.name = $"ITEM_{subtoken}_NAME";
        Item.nameToken = $"ITEM_{subtoken}_NAME";
        Item.pickupToken = $"ITEM_{subtoken}_PICKUP";
        Item.descriptionToken = $"ITEM_{subtoken}_DESC";
        
        // No lore for consumed item
        if (GetTier() != ItemTier.NoTier)
            Item.loreToken = $"ITEM_{subtoken}_LORE";
        
        Item.tier = GetTier();
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