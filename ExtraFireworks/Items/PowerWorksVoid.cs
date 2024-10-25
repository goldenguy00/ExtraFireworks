using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine.Networking;
using VoidItemAPI;

namespace ExtraFireworks.Items
{
    public class PowerWorksVoid : ItemBase<PowerWorksVoid>
    {
        public ConfigEntry<int> fireworksPerStack;
        public ConfigEntry<float> hpThreshold;

        public PowerWorksVoidConsumed ConsumedItem;
        private bool voidInitialized = false;

        public PowerWorksVoid() : base()
        {
            ConsumedItem = new PowerWorksVoidConsumed(this);
            fireworksPerStack = PluginConfig.config.Bind(ConfigSection, "FireworksPerUse", 20,
                "Number of fireworks per consumption");
            hpThreshold = PluginConfig.config.Bind(ConfigSection, "HpThreshold", 0.25f,
                "HP threshold before Power Works is consumed");
        }

        public override string UniqueName => "PowerWorks";

        public override string PickupModelName => "Power Works.prefab";

        public override float ModelScale => 0.4f;

        public override string PickupIconName => "PowerWorks.png";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

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

        public override void AddHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += this.HealthComponent_TakeDamage;

            On.RoR2.ItemCatalog.SetItemDefs += this.ItemCatalog_SetItemDefs;

            On.RoR2.CharacterMaster.OnServerStageBegin += this.CharacterMaster_OnServerStageBegin; ;
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
            body.inventory.GiveItem(ConsumedItem.Item, count);
            CharacterMasterNotificationQueue.SendTransformNotification(body.master, Item.itemIndex,
                ConsumedItem.Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);

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

            var consumedCount = self.inventory.GetItemCount(ConsumedItem.Item);
            if (consumedCount <= 0)
                return;

            self.inventory.RemoveItem(ConsumedItem.Item, consumedCount);
            self.inventory.GiveItem(Item, consumedCount);
            CharacterMasterNotificationQueue.SendTransformNotification(self, ConsumedItem.Item.itemIndex,
                Item.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
        }

        private void ItemCatalog_SetItemDefs(On.RoR2.ItemCatalog.orig_SetItemDefs orig, ItemDef[] newItemDefs)
        {
            orig(newItemDefs);

            if (!voidInitialized)
            {
                VoidTransformation.CreateTransformation(Item, DLC1Content.Items.HealingPotion);
                voidInitialized = true;
            }
        }
    }
}