using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkDroneWeapon(FireworkItem parent) : BaseFireworkItem<FireworkDroneWeapon>()
    {
        private readonly FireworkItem parent = parent;

        public override string GetName() => "FireworkDroneWeapon";

        public override string GetPickupModelName() => parent.GetPickupModelName();

        public override float GetModelScale() => parent.GetModelScale();

        public override string GetPickupIconName() => parent.GetPickupIconName();

        public override ItemTier GetTier() => ItemTier.NoTier;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.BrotherBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate, ItemTag.CannotSteal];

        public override string GetItemName() => "Drone Firework Weapon";

        public override string GetItemPickup() => parent.GetItemPickup();

        public override string GetItemDescription() => parent.GetItemDescription();

        public override string GetItemLore() => parent.GetItemLore();

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
