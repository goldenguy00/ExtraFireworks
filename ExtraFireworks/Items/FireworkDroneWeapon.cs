using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDroneWeapon(ItemBase parent) : ItemBase<FireworkDroneWeapon>()
    {
        private readonly ItemBase parent = parent;

        public override string ItemName => "Drone Firework Weapon";

        public override string UniqueName => "FireworkDroneWeapon";

        public override string PickupModelName => string.Empty;

        public override string PickupIconName => parent.PickupIconName;

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string ItemPickupDescription => string.Empty;

        public override string ItemDescription => string.Empty;

        public override string ItemLore => string.Empty;

        public override void AddHooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += this.CharacterBody_OnInventoryChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active)
                self.AddItemBehavior<FireworkDroneWeaponBehaviour>(self.inventory.GetItemCount(this.Item));
        }
    }

    public class FireworkDroneWeaponBehaviour : CharacterBody.ItemBehavior
    {
        private float timer;

        private void Awake()
        {
            this.enabled = false;
        }

        private void OnEnable()
        {
            timer = Random.Range(0, FireworkDrones.fireworkInterval.Value);
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (this.body && this.stack > 0 && timer > FireworkDrones.fireworkInterval.Value)
            {
                timer = 0;
                ExtraFireworks.FireFireworks(this.body, FireworkDrones.scaler.GetValueInt(stack));
            }
        }
    }
}
