using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDroneWeapon(ItemBase parent) : ItemBase<FireworkDroneWeapon>()
    {
        private readonly ItemBase parent = parent;

        public override string UniqueName => "FireworkDroneWeapon";

        public override string PickupModelName => parent.PickupModelName;

        public override float ModelScale => parent.ModelScale;

        public override string PickupIconName => parent.PickupIconName;

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string ItemName => "Drone Firework Weapon";

        public override string ItemPickupDescription => parent.ItemPickupDescription;

        public override string ItemDescription => parent.ItemDescription;

        public override string ItemLore => parent.ItemLore;

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
