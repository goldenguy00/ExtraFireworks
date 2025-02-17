﻿using BepInEx.Configuration;
using ExtraFireworks.Config;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoid : ItemBase<PowerWorksVoid>
    {
        public ConfigEntry<int> fireworksPerStack;
        public ConfigEntry<float> hpThreshold;

        public PowerWorksVoid() : base()
        {
            fireworksPerStack = PluginConfig.BindOptionSlider(ConfigSection, 
                "FireworksPerUse",
                20,
                "Number of fireworks per consumption",
                1, 100);

            hpThreshold = PluginConfig.BindOptionSlider(ConfigSection,
                "HpThreshold",
                0.25f,
                "HP threshold before Power Works is consumed",
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

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);

            new PowerWorksVoidConsumed().Init(bundle);

            var healingPotion = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/HealingPotion/HealingPotion.asset").WaitForCompletion();
            var healingPotionConsumed = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/HealingPotion/HealingPotionConsumed.asset").WaitForCompletion();

            var provider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            provider.name = "ExtraFireworksContagiousItemProvider";
            provider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
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

        public override void AddHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += this.HealthComponent_TakeDamage;
            On.RoR2.CharacterMaster.OnServerStageBegin += this.CharacterMaster_OnServerStageBegin;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info)
        {
            orig(self, info);

            if (!NetworkServer.active)
                return;

            // Check if HP threshold met
            if (self.health / self.fullHealth > hpThreshold.Value)
                return;

            var body = self.body;
            if (!body || !body.inventory || !body.master)
                return;

            var count = body.inventory.GetItemCount(Item.itemIndex);
            if (count <= 0)
                return;

            body.inventory.RemoveItem(Item, count);
            body.inventory.GiveItem(PowerWorksVoidConsumed.Instance.Item, count);
            CharacterMasterNotificationQueue.SendTransformNotification(body.master, Item.itemIndex,
                PowerWorksVoidConsumed.Instance.Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);

            ExtraFireworks.FireFireworks(body, fireworksPerStack.Value * count);

            // Also give void bubble
            body.SetBuffCount(DLC1Content.Buffs.BearVoidCooldown.buffIndex, 0);
            body.SetBuffCount(DLC1Content.Buffs.BearVoidReady.buffIndex, 1);
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);

            if (!self.inventory)
                return;

            var consumedCount = self.inventory.GetItemCount(PowerWorksVoidConsumed.Instance.Item);
            if (consumedCount <= 0)
                return;

            self.inventory.RemoveItem(PowerWorksVoidConsumed.Instance.Item, consumedCount);
            self.inventory.GiveItem(Item, consumedCount);
            CharacterMasterNotificationQueue.SendTransformNotification(self, PowerWorksVoidConsumed.Instance.Item.itemIndex,
                Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
        }
    }
}