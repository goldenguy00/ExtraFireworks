using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDrones : ItemBase<FireworkDrones>
    {
        internal static ConfigEntry<float> fireworkInterval;
        internal static ConfigurableLinearScaling scaler;

        public FireworkDrones() : base()
        {
            new FireworkDroneWeapon(this);

            fireworkInterval = PluginConfig.config.Bind(ConfigSection, "FireworksInterval", 4f,
                "Number of seconds between bursts of fireworks");
            scaler = new ConfigurableLinearScaling(ConfigSection, 4, 2);
        }

        public override string UniqueName => "FireworkDrones";

        public override string PickupModelName => "Spare Fireworks.prefab";

        public override float ModelScale => 1f;

        public override string PickupIconName => "SpareFireworks.png";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Spare Fireworks";

        public override string ItemPickupDescription => "All drones now shoot fireworks";

        public override string ItemDescription => $"<style=cIsUtility>Non-player allies</style> gain an " +
                   $"<style=cIsDamage>automatic firework launcher</style> that propels " +
                   $"<style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> " +
                   $"<style=cIsDamage>fireworks every {fireworkInterval.Value} seconds</style> " +
                   $"for <style=cIsDamage>300%</style> base damage each.";

        public override string ItemLore => "Ayo what we do with all these fireworks?! *END TRANSMISSION*";

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
            this.UpdateAllMinions(this.body.inventory.GetItemCount(FireworkDrones.Instance.Item));
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
            if (!base.body || !base.body.master || !(base.body.master == summonReport.leaderMasterInstance))
                return;

            var minionMaster = summonReport.summonMasterInstance;
            if (minionMaster)
            {
                this.UpdateMinionInventory(minionMaster.inventory, base.stack);
            }
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
                int itemCount = inventory.GetItemCount(FireworkDroneWeapon.Instance.Item);
                if (itemCount < base.stack)
                {
                    inventory.GiveItem(FireworkDroneWeapon.Instance.Item, base.stack - itemCount);
                }
                else if (itemCount > base.stack)
                {
                    inventory.RemoveItem(FireworkDroneWeapon.Instance.Item, itemCount - base.stack);
                }
            }
            else
            {
                inventory.ResetItem(FireworkDroneWeapon.Instance.Item);
            }
        }
    }
}