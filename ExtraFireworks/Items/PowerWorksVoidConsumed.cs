using RoR2;
using UnityEngine;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoidConsumed : ItemBase<PowerWorksVoidConsumed>
    {
        public override string ItemName => "Power 'Works (Consumed)";

        public override string UniqueName => "PowerWorksConsumed";

        public override string PickupModelName => string.Empty;

        public override string PickupIconName => "PowerWorksConsumed.png";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.Damage];

        public override string ItemPickupDescription => string.Empty;

        public override string ItemDescription => string.Empty;

        public override string ItemLore => string.Empty;

        public override bool RequireSotV => true;

        public override void AddHooks() { }
        public override void AdjustPickupModel() { }
        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);
        }
    }
}