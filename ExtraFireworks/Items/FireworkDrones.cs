using BepInEx.Configuration;
using MiscFixes.Modules;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDrones : ItemBase<FireworkDrones>
    {
        internal static ConfigEntry<float> fireworkInterval;
        internal static ConfigurableLinearScaling scaler;

        public FireworkDrones() : base()
        {
            fireworkInterval = ExtraFireworks.instance.Config.BindOptionSlider(ConfigSection,
                "FireworksInterval",
                "Number of seconds between bursts of fireworks",
                4f,
                1, 10);
            scaler = new ConfigurableLinearScaling(ConfigSection, 4, 2);
        }

        public override string UniqueName => "FireworkDrones";

        public override string PickupModelName => "Spare Fireworks.prefab";

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

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);

            new FireworkDroneWeapon().Init(bundle);
        }
    }

    public class FireworkDroneBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => FireworkDrones.Instance.Item;

        private void Start()
        {
            this.UpdateAllMinions(this.stack);
            MinionOwnership.onMinionGroupChangedGlobal += MinionOwnership_onMinionGroupChangedGlobal;
        }

        private void OnDestroy()
        {
            MinionOwnership.onMinionGroupChangedGlobal -= MinionOwnership_onMinionGroupChangedGlobal;
            UpdateAllMinions();
        }

        public override void OnInventoryRefresh()
        {
            UpdateAllMinions(base.stack);
        }

        private void MinionOwnership_onMinionGroupChangedGlobal(MinionOwnership minionGroup)
        {
            UpdateAllMinions(base.stack);
        }

        private void UpdateAllMinions(int newStack = 0)
        {
            if (!body || !body.master)
                return;

            var minionGroup = MinionOwnership.MinionGroup.FindGroup(base.body.master.netId);
            if (minionGroup == null)
                return;

            foreach (var minionOwnership in minionGroup.members)
            {
                if (minionOwnership && minionOwnership.TryGetComponent<CharacterMaster>(out var master))
                {
                    this.UpdateMinionInventory(master, newStack);
                }
            }
        }

        private void UpdateMinionInventory(CharacterMaster master, int newStack = 0)
        {
            if (!master || !master.inventory)
                return;

            int itemCount = master.inventory.GetItemCountPermanent(FireworkDroneWeapon.Instance.Item);
            if (newStack != itemCount)
            {
                master.inventory.GiveItemPermanent(FireworkDroneWeapon.Instance.Item, newStack - itemCount);
            }
        }
    }
}