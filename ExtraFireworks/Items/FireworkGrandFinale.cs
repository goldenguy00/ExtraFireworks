using BepInEx.Configuration;
using ExtraFireworks.Config;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{

    public class FireworkGrandFinale : ItemBase<FireworkGrandFinale>
    {
        internal readonly ConfigEntry<float> fireworkDamage;
        internal readonly ConfigEntry<float> fireworkExplosionSize;
        internal readonly ConfigEntry<int> fireworkEnemyKillcount;

        internal BuffDef buff;

        internal GameObject projectilePrefab;
        internal GameObject grandFinaleModel, ghost;

        public FireworkGrandFinale() : base()
        {
            fireworkDamage = PluginConfig.BindOptionSteppedSlider(ConfigSection,
                "DamageCoefficient",
                50f,
                1,
                "Damage of Grand Finale firework as coefficient of base damage",
                1, 100);

            fireworkExplosionSize = PluginConfig.BindOptionSteppedSlider(ConfigSection,
                "ExplosionRadius",
                10f,
                1,
                "Explosion radius of Grand Finale firework",
                1, 20);

            fireworkEnemyKillcount = PluginConfig.BindOptionSlider(ConfigSection,
                "KillThreshold",
                10,
                "Number of enemies required to proc the Grand Finale firework",
                1, 20);
        }

        public override string UniqueName => "FireworkGrandFinale";

        public override string PickupModelName => "GrandFinale.prefab";

        public override string PickupIconName => "GrandFinale.png";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Grand Finale";

        public override string ItemPickupDescription => $"Launch a grand finale firework after killing {fireworkEnemyKillcount.Value} enemies.";

        public override string ItemDescription => $"<style=cIsDamage>Killing {fireworkEnemyKillcount.Value}</style> " +
                   $"<style=cStack>(-50% per stack)</style> <style=cIsDamage>enemies</style> fires out a " +
                   $"<style=cIsDamage>massive firework</style> that deals <style=cIsDamage>5000%</style> base damage.";

        public override string ItemLore => "Ayo what we do this one big ass firework?! *END TRANSMISSION*";

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);

            ghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkGhost.prefab").WaitForCompletion().InstantiateClone("GrandFinaleGhost", true);
            GameObject.Destroy(ghost.transform.Find("mdlFireworkProjectile").gameObject);

            var model = bundle.LoadAsset<GameObject>("Assets/ImportModels/GrandFinaleProjectile.prefab").transform.Find("GameObject").Find("GrandFinaleProjectile");
            grandFinaleModel = GameObject.Instantiate(model.gameObject, ghost.transform);
            grandFinaleModel.transform.SetAsFirstSibling();
            grandFinaleModel.name = "mdlGrandFinaleProjectile";

            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = "GrandFinaleCountdown";
            buff.canStack = true;
            buff.isCooldown = false;
            buff.isDebuff = false;
            buff.buffColor = Color.red;
            buff.iconSprite = bundle.LoadAsset<Sprite>("Assets/Import/GrandFinaleBuff.png"); ;
            ContentAddition.AddBuffDef(buff);

            projectilePrefab = ExtraFireworks.fireworkPrefab.InstantiateClone("GrandFinaleProjectile", true);
            projectilePrefab.GetComponent<ProjectileController>().ghostPrefab = ghost;

            //projectilePrefab.transform.localScale *= 5;
            projectilePrefab.layer = LayerIndex.projectile.intVal;
            var pie = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.lifetime = 99f;
            pie.explodeOnLifeTimeExpiration = true;
            pie.blastDamageCoefficient = 1f;
            pie.blastProcCoefficient = 1f;

            var originalBlastRadius = pie.blastRadius;
            pie.blastRadius = fireworkExplosionSize.Value;

            pie.dotDamageMultiplier = 1f;
            pie.canRejectForce = true;
            pie.falloffModel = BlastAttack.FalloffModel.None;

            /*if (pie.impactEffect)
            {
                var newImpactEffect = pie.impactEffect.InstantiateClone("GrandFinaleImpact");
                newImpactEffect.transform.localScale *= fireworkExplosionSize.Value / originalBlastRadius;
                newImpactEffect.AddComponent<NetworkIdentity>();
                newImpactEffect.SetActive(false);
                pie.impactEffect = newImpactEffect;
            }

            if (pie.explosionEffect)
            {
                var newExplosionEffect = pie.explosionEffect.InstantiateClone("GrandFinaleExplosion");
                newExplosionEffect.transform.localScale *= fireworkExplosionSize.Value / originalBlastRadius;
                newExplosionEffect.SetActive(false);
                pie.explosionEffect = newExplosionEffect;
            }*/

            //pie.impactEffect = "FireworkExplosion2";
            var missileController = projectilePrefab.GetComponent<MissileController>();
            missileController.maxVelocity = 7.5f;
            missileController.maxSeekDistance = 150f;
            missileController.acceleration = 2f;
            missileController.rollVelocity = 3f;
            missileController.giveupTimer = 99f;
            missileController.deathTimer = 99f;
            missileController.turbulence = 0.5f;

            var boxCollider = projectilePrefab.GetComponent<BoxCollider>();
            boxCollider.size *= 5f;
        }

        public override void AddHooks()
        {
            // Implement fireworks on kill
            RoR2.GlobalEventManager.onCharacterDeathGlobal += this.GlobalEventManager_OnCharacterDeath;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += this.CharacterBody_onBodyInventoryChangedGlobal;

        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (NetworkServer.active)
            {
                if (body.HasBuff(this.buff) && body.inventory.GetItemCountEffective(this.Item) <= 0)
                    body.RemoveBuff(this.buff);
            }
        }

        private void GlobalEventManager_OnCharacterDeath(DamageReport report)
        {
            if (!NetworkServer.active || report == null)
                return;

            if (!report.attackerBody || !report.attackerMaster || !report.attackerMaster.inventory)
                return;

            var count = report.attackerMaster.inventory.GetItemCountEffective(Item);
            if (count > 0)
            {
                var body = report.attackerBody;
                var buffCount = body.GetBuffCount(this.buff);

                if (buffCount <= 0)
                {
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefab,
                        position = body.corePosition + Vector3.up * body.radius,
                        rotation = Quaternion.LookRotation(Vector3.up),
                        owner = body.gameObject,
                        damage = fireworkDamage.Value * body.baseDamage,
                        force = 500f,
                        crit = body.RollCrit()
                    });
                    body.SetBuffCount(this.buff.buffIndex, this.fireworkEnemyKillcount.Value);
                }
                else
                {
                    body.RemoveBuff(this.buff);
                }
            }
        }
    }
}