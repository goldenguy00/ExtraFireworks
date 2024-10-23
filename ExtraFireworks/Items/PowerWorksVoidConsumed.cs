using RoR2;
using VoidItemAPI;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoidConsumed(PowerWorksVoid parent) : BaseFireworkItem<PowerWorksVoidConsumed>()
    {
        private readonly PowerWorksVoid parent = parent;
        private bool voidInitialized = false;

        public override string GetName() => "PowerWorksConsumed";

        public override string GetPickupModelName() => "Power Works.prefab";

        public override float GetModelScale() => 0.4f;

        public override string GetPickupIconName() => "PowerWorksConsumed.png";

        public override ItemTier GetTier() => ItemTier.NoTier;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string GetItemName() => "Power 'Works (Consumed)";

        public override string GetItemPickup() => parent.GetItemPickup();

        public override string GetItemDescription() => parent.GetItemDescription();

        public override string GetItemLore() => parent.GetItemLore();

        public override void AddHooks()
        {
            On.RoR2.ItemCatalog.SetItemDefs += this.ItemCatalog_SetItemDefs;
        }

        private void ItemCatalog_SetItemDefs(On.RoR2.ItemCatalog.orig_SetItemDefs orig, ItemDef[] newItemDefs)
        {
            orig(newItemDefs);

            if (!voidInitialized)
            {
                VoidTransformation.CreateTransformation(Item, DLC1Content.Items.HealingPotionConsumed);
                voidInitialized = true;
            }
        }
    }
}