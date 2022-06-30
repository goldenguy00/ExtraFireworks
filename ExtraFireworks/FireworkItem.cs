using BepInEx.Configuration;
using R2API;
using RoR2;
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
    public abstract ItemTiers GetTier();
    public abstract ItemTag[] GetTags();
    public abstract float GetModelScale();
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
        return $"Assets/Import/{GetPickupModelName()}";
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
        itemEnabled = config.Bind(GetConfigSection(), "Enabled", true, "Item enabled?");
        
        Item = ScriptableObject.CreateInstance<ItemDef>();
        
        var subtoken = GetName().ToUpper();
        Item.name = $"ITEM_{subtoken}_NAME";
        Item.nameToken = $"ITEM_{subtoken}_NAME";
        Item.pickupToken = $"ITEM_{subtoken}_PICKUP";
        Item.descriptionToken = $"ITEM_{subtoken}_DESC";
        Item.loreToken = $"ITEM_{subtoken}_LORE";
        
        Item._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>($"RoR2/Base/Common/{GetTier()}Def.asset").WaitForCompletion();
        Item.canRemove = true;
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
            LanguageAPI.Add(Item.loreToken, GetItemLore());
            
            ItemAPI.Add(new CustomItem(Item, GetDisplayRules()));
            
            AddHooks();
        }
    }
}