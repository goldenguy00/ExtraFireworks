using BepInEx.Configuration;
using MiscFixes.Modules;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoid : ItemBase<PowerWorksVoid>
    {
        public ConfigEntry<int> fireworksPerStack;
        public ConfigEntry<float> hpThreshold;

        public PowerWorksVoid() : base()
        {
            fireworksPerStack = ExtraFireworks.instance.Config.BindOptionSlider(ConfigSection,
                "FireworksPerUse",
                "Number of fireworks per consumption",
                20,
                1, 100);

            hpThreshold = ExtraFireworks.instance.Config.BindOptionSlider(ConfigSection,
                "HpThreshold",
                "HP threshold before Power Works is consumed",
                0.25f,
                0, 1);
        }

        public override string UniqueName => "PowerWorks";

        public override string PickupModelName => "Power Works.prefab";

        public override Vector3? ModelScale => Vector3.one;

        public override string PickupIconName => "PowerWorks.png";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.LowHealth, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Power 'Works";

        public override string ItemPickupDescription => "Release a barrage of fireworks at low health. Refreshes every stage. Corrupts all Power Elixirs.";

        public override string ItemDescription =>
                    $"Taking damage to below <style=cIsHealth>{hpThreshold.Value * 100:0}% health</style> " +
                   $"<style=cIsUtility>consumes</style> this item, releasing a " +
                   $"<style=cIsDamage>barrage of fireworks</style> dealing " +
                   $"<style=cIsDamage>{fireworksPerStack.Value}x300%</style> " +
                   $"<style=cStack>(+{fireworksPerStack.Value} per stack)</style> base damage. " +
                   $"<style=cIsUtility>(Refreshes next stage)</style>. <style=cIsVoid>Corrupts all Power Elixirs</style>.";

        public override string ItemLore => "MMMM YUM.";

        public override bool RequireSotV => true;
        public override bool CanBeTemp => false;

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);

            new PowerWorksVoidConsumed().Init(bundle);
            
            var healingPotion = Addressables.LoadAssetAsync<ItemDef>(RoR2_DLC1_HealingPotion.HealingPotion_asset).WaitForCompletion();
            var healingPotionConsumed = Addressables.LoadAssetAsync<ItemDef>(RoR2_DLC1_HealingPotion.HealingPotionConsumed_asset).WaitForCompletion();

            var provider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            provider.name = "ExtraFireworksContagiousItemProvider";
            provider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>(RoR2_DLC1_Common.ContagiousItem_asset).WaitForCompletion();
            provider.relationships =
            [
                new ItemDef.Pair
                {
                    itemDef1 = healingPotion,
                    itemDef2 = this.Item
                },
                new ItemDef.Pair
                {
                    itemDef1 = healingPotionConsumed,
                    itemDef2 = PowerWorksVoidConsumed.Instance.Item
                }
            ];

            ContentAddition.AddItemRelationshipProvider(provider);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public override void AdjustPickupModel()
        {
            base.AdjustPickupModel();

            var prefab = this.Item?.pickupModelPrefab;
            if (prefab)
            {
                var mdlFireworks = prefab.transform.Find("mdlFireworks");
                var mdlPotion = prefab.transform.Find("mdlHealingPotion");
                var mdlLiquid = mdlPotion.Find("mdlHealingPotionCorkLiquid");

                mdlPotion.localScale = Vector3.one;
                mdlLiquid.localScale = Vector3.one;

                mdlFireworks.SetParent(mdlPotion);
                mdlFireworks.localScale = Vector3.one * 0.4f;
                mdlFireworks.localPosition = new Vector3(-0.2f, 0.05f, 3.5f);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public class PowerWorksBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => PowerWorksVoid.Instance?.Item;

        private void Start()
        {
            body?.healthComponent?.AddOnTakeDamageServerReceiver(this);
        }

        private void OnDestroy()
        {
            body?.healthComponent?.RemoveOnTakeDamageServerReceiver(this);
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (!body.inventory)
                return;

            // Check if HP threshold met
            if (body.healthComponent.healthFraction > PowerWorksVoid.Instance.hpThreshold.Value)
                return;

            if (body.inventory && new Inventory.ItemTransformation
            {
                allowWhenDisabled = false,
                forbidPermanentItems = false,
                forbidTempItems = false,
                originalItemIndex = PowerWorksVoid.Instance.Item.itemIndex,
                newItemIndex = PowerWorksVoidConsumed.Instance.Item.itemIndex,
                minToTransform = 1,
                maxToTransform = int.MaxValue,
                transformationType = (ItemTransformationTypeIndex)3
            }.TryTransform(body.inventory, out var result))
            {
                CharacterMasterNotificationQueue.SendTransformNotification(body.master, result.takenItem.itemIndex, 
                    result.givenItem.itemIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);

                ExtraFireworks.FireFireworks(body, PowerWorksVoid.Instance.fireworksPerStack.Value * result.totalTransformed);

                // Also give void bubble
                if (body.HasBuff(DLC1Content.Buffs.BearVoidCooldown))
                    body.SetBuffCount(DLC1Content.Buffs.BearVoidCooldown.buffIndex, 0);

                if (!body.HasBuff(DLC1Content.Buffs.BearVoidReady))
                    body.AddBuff(DLC1Content.Buffs.BearVoidReady);
            }
        }
    }
}