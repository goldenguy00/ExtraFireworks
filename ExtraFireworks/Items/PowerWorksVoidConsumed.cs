using RoR2;
using VoidItemAPI;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoidConsumed(PowerWorksVoid parent) : ItemBase<PowerWorksVoidConsumed>()
    {
        private readonly PowerWorksVoid parent = parent;
        private bool voidInitialized = false;

        public override string UniqueName => "PowerWorksConsumed";

        public override string PickupModelName => "Power Works.prefab";

        public override float ModelScale => 0.4f;

        public override string PickupIconName => "PowerWorksConsumed.png";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string ItemName => "Power 'Works (Consumed)";

        public override string ItemPickupDescription => parent.ItemPickupDescription;

        public override string ItemDescription => parent.ItemDescription;

        public override string ItemLore => parent.ItemLore;

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