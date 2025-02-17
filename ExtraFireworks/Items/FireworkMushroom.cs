﻿using ExtraFireworks.Config;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using R2API;

namespace ExtraFireworks.Items
{
    public class FireworkMushroom : ItemBase<FireworkMushroom>
    {
        internal readonly ConfigurableHyperbolicScaling scaler;

        public FireworkMushroom() : base()
        {
            scaler = new ConfigurableHyperbolicScaling(ConfigSection, 2, 0.1f);
        }

        public override string UniqueName => "FireworkMushroom";

        public override string PickupModelName => "Fungus.prefab";

        public override string PickupIconName => "Fungus.png";

        public override Vector3? ModelScale => Vector3.one * 1.5f;

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Fungus";

        public override string ItemPickupDescription => "Become a firework launcher when you stand still.";

        public override string ItemDescription =>
                $"After <style=cIsUtility>standing still</style> for <style=cIsUtility>1 second</style>, shoot fireworks " +
                $"at <style=cIsDamage>{scaler.GetValue(1) * 100:0}%</style> " +
                $"<style=cStack>(+{(scaler.GetValue(2) - scaler.GetValue(1)) * 100:0} per stack)</style> speed " +
                $"<style=cStack>(hyperbolic up to 100%)</style> that deal <style=cIsDamage>300%</style> base damage.";

        public override string ItemLore => "A fun arts and crafts project.";

        public override void AdjustPickupModel()
        {
            base.AdjustPickupModel();

            var prefab = this.Item?.pickupModelPrefab;
            if (prefab)
            {
                prefab.transform.Find("MushroomMesh").SetAsFirstSibling();
            }
        }

        public override void AddHooks()
        {
        }
    }

    public class FireworkWard : NetworkBehaviour
    {
        public Transform rangeIndicator;
        public bool floorWard;
        
        public CharacterBody body;
        public int stack;
        public float radius;
        public float interval = 1f;

        private float fireTimer;
        private float rangeIndicatorScaleVelocity;

        private void Awake()
        {
            if (NetworkServer.active)
            {
                if (this.floorWard && Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 500f, LayerIndex.world.mask))
                {
                    base.transform.position = hitInfo.point;
                    base.transform.up = hitInfo.normal;
                }
            }
        }

        private void Update()
        {
            if (this.rangeIndicator)
            {
                float num = Mathf.SmoothDamp(this.rangeIndicator.localScale.x, this.radius, ref this.rangeIndicatorScaleVelocity, 0.2f);
                this.rangeIndicator.localScale = num * Vector3.one;
            }
        }

        private void FixedUpdate()
        {
            this.fireTimer -= Time.fixedDeltaTime;
            if (this.fireTimer <= 0f)
            {
                this.fireTimer = this.interval;
                if (NetworkServer.active && body)
                {
                    ExtraFireworks.FireFireworks(body, 1);
                }
            }
        }
    }

    public class FireworkBodyBehavior : BaseItemBodyBehavior
    {
        private static GameObject fireworkWardPrefab;

        private GameObject fireworkWardGameObject;

        private FireworkWard fireworkWard;

        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return FireworkMushroom.Instance.Item;
        }

        [InitDuringStartup]
        private static new void Init()
        {
            fireworkWardPrefab = LegacyResourcesAPI.LoadAsync<GameObject>("Prefabs/NetworkedObjects/MushroomWard").WaitForCompletion().InstantiateClone("FireworkWard");
            var oldWard = fireworkWardPrefab.GetComponent<HealingWard>();
            var newWard = fireworkWardPrefab.AddComponent<FireworkWard>();
            newWard.rangeIndicator = oldWard.rangeIndicator;
            newWard.floorWard = oldWard.floorWard;
            Destroy(oldWard);
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            bool isNotMoving = base.stack > 0 && base.body.GetNotMoving();
            float networkradius = base.body.radius + 3f;
            if (this.fireworkWardGameObject != isNotMoving)
            {
                if (isNotMoving)
                {
                    this.fireworkWardGameObject = Object.Instantiate(FireworkBodyBehavior.fireworkWardPrefab, base.body.footPosition, Quaternion.identity);
                    this.fireworkWard = this.fireworkWardGameObject.GetComponent<FireworkWard>();
                    NetworkServer.Spawn(this.fireworkWardGameObject);
                }
                else
                {
                    Object.Destroy(this.fireworkWardGameObject);
                    this.fireworkWardGameObject = null;
                }
            }
            if (this.fireworkWard)
            {
                this.fireworkWard.radius = networkradius;
                this.fireworkWard.body = this.body;
                this.fireworkWard.stack = base.stack;
                this.fireworkWard.interval = 1f - FireworkMushroom.Instance.scaler.GetValue(base.stack);
            }
        }

        private void OnDisable()
        {
            if (this.fireworkWardGameObject)
            {
                Object.Destroy(this.fireworkWardGameObject);
            }
        }
    }
}