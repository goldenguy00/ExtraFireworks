﻿using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class ItemFireworkDrones : FireworkItem<ItemFireworkDrones>
    {
        internal static ConfigEntry<float> fireworkInterval;
        internal static ConfigurableLinearScaling scaler;

        public ItemFireworkDrones() : base()
        {
            new ItemFireworkDroneWeapon(this);
            fireworkInterval = PluginConfig.config.Bind(GetConfigSection(), "FireworksInterval", 4f,
                "Number of seconds between bursts of fireworks");
            scaler = new ConfigurableLinearScaling("", GetConfigSection(), 4, 2);
        }

        public override string GetName() => "FireworkDrones";

        public override string GetPickupModelName() => "Spare Fireworks.prefab";

        public override float GetModelScale() => 1f;

        public override string GetPickupIconName() => "SpareFireworks.png";

        public override ItemTier GetTier() => ItemTier.Tier3;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string GetItemName() => "Spare Fireworks";

        public override string GetItemPickup() => "All drones now shoot fireworks";

        public override string GetItemDescription()
        {
            return $"<style=cIsUtility>Non-player allies</style> gain an " +
                   $"<style=cIsDamage>automatic firework launcher</style> that propels " +
                   $"<style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> " +
                   $"<style=cIsDamage>fireworks every {fireworkInterval.Value} seconds</style> " +
                   $"for <style=cIsDamage>300%</style> base damage each.";
        }

        public override string GetItemLore() => "Ayo what we do with all these fireworks?! *END TRANSMISSION*";

        public override void AddHooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += this.CharacterBody_OnInventoryChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active)
                self.AddItemBehavior<FireworkDroneBehaviour>(self.inventory.GetItemCount(this.Item));
        }
    }

    public class FireworkDroneBehaviour : CharacterBody.ItemBehavior
    {
        private int previousStack;

        private void Awake()
        {
            this.enabled = false;
        }

        private void OnEnable()
        {
            this.UpdateAllMinions(this.body.inventory.GetItemCount(ItemFireworkDrones.Instance.Item));
            MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
        }

        private void OnDisable()
        {
            MasterSummon.onServerMasterSummonGlobal -= OnServerMasterSummonGlobal;
            this.UpdateAllMinions();
        }

        private void FixedUpdate()
        {
            if (this.previousStack != base.stack)
            {
                this.UpdateAllMinions(base.stack);
            }
        }

        private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
        {
            this.UpdateAllMinions(base.stack);
        }

        private void UpdateAllMinions(int newStack = 0)
        {
            if (!base.body || !base.body.master)
                return;

            var minionGroup = MinionOwnership.MinionGroup.FindGroup(base.body.master.netId);
            if (minionGroup == null)
                return;

            foreach (var minionOwnership in minionGroup.members)
            {
                if (minionOwnership && minionOwnership.TryGetComponent<CharacterMaster>(out var master))
                {
                    this.UpdateMinionInventory(master.inventory, newStack);
                }
            }

            this.previousStack = newStack;
        }

        private void UpdateMinionInventory(Inventory inventory, int newStack = 0)
        {
            if (!inventory)
                return;

            if (newStack > 0)
            {
                int itemCount = inventory.GetItemCount(ItemFireworkDroneWeapon.Instance.Item);
                if (itemCount < base.stack)
                {
                    inventory.GiveItem(ItemFireworkDroneWeapon.Instance.Item, base.stack - itemCount);
                }
                else if (itemCount > base.stack)
                {
                    inventory.RemoveItem(ItemFireworkDroneWeapon.Instance.Item, itemCount - base.stack);
                }
            }
            else
            {
                inventory.ResetItem(ItemFireworkDroneWeapon.Instance.Item);
            }
        }
    }
}