using RoR2;
using RoR2.Items;
using UnityEngine;

namespace ExtraFireworks.Items
{
    public class FireworkDroneWeapon : ItemBase<FireworkDroneWeapon>
    {
        public override string ItemName => "Drone Firework Weapon";

        public override string UniqueName => "FireworkDroneWeapon";

        public override string PickupModelName => string.Empty;

        public override string PickupIconName => string.Empty;

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.BrotherBlacklist];

        public override string ItemPickupDescription => string.Empty;

        public override string ItemDescription => string.Empty;

        public override string ItemLore => string.Empty;

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);
        }
    }

    public class FireworkDroneWeaponBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => FireworkDroneWeapon.Instance?.Item;

        private float timer;

        private void Start()
        {
            timer = Random.Range(0, FireworkDrones.fireworkInterval.Value);
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer > FireworkDrones.fireworkInterval.Value)
            {
                timer = 0;
                ExtraFireworks.FireFireworks(this.body, FireworkDrones.scaler.GetValueInt(stack));
            }
        }
    }
}
