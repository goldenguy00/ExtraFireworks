using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;
using VoidItemAPI;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoidConsumed(PowerWorksVoid parent) : ItemBase<PowerWorksVoidConsumed>()
    {
        private readonly PowerWorksVoid parent = parent;
        private bool voidInitialized = false;

        public override string ItemName => "Power 'Works (Consumed)";

        public override string UniqueName => "PowerWorksConsumed";

        public override string PickupModelName => string.Empty;

        public override string PickupIconName => "PowerWorksConsumed.png";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string ItemPickupDescription => string.Empty;

        public override string ItemDescription => string.Empty;

        public override string ItemLore => string.Empty;

        public override void AddHooks() { }
        public override void AdjustPickupModel() { }
        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);

            VoidTransformation.CreateTransformation(Item, "HealingPotionConsumed");
        }
    }
}