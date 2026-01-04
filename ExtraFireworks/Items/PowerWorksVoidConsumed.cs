using RoR2;
using UnityEngine;
using UnityEngine.Networking;

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

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);
        }

        public override void AddHooks()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += this.CharacterMaster_OnServerStageBegin;
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);

            if (self.inventory && new Inventory.ItemTransformation
            {
                allowWhenDisabled = true,
                forbidPermanentItems = false,
                forbidTempItems = false,
                originalItemIndex = PowerWorksVoidConsumed.Instance.Item.itemIndex,
                newItemIndex = PowerWorksVoid.Instance.Item.itemIndex,
                minToTransform = 1,
                maxToTransform = int.MaxValue,
                transformationType = (ItemTransformationTypeIndex)0
            }.TryTransform(self.inventory, out var result))
            {
                CharacterMasterNotificationQueue.SendTransformNotification(self, result.takenItem.itemIndex,
                    result.givenItem.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            }
        }
    }
}