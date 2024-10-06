using R2API;
using RoR2.Projectile;

namespace ExtraFireworks;

using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class ItemFireworkFinale : FireworkItem
{
    private ConfigEntry<float> fireworkDamage;
    private ConfigEntry<float> fireworkExplosionSize;
    private ConfigEntry<int> fireworkEnemyKillcount;

    private GameObject projectilePrefab;
    
    public ItemFireworkFinale(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworkDamage = config.Bind(GetConfigSection(), "DamageCoefficient", 30f,
            "Damage of Grand Finale firework as coefficient of base damage");
        fireworkExplosionSize = config.Bind(GetConfigSection(), "ExplosionRadius", 10f,
            "Explosion radius of Grand Finale firework");
        fireworkEnemyKillcount = config.Bind(GetConfigSection(), "KillThreshold", 15,
            "Number of enemies required to proc the Grand Finale firework");
    }

    public override string GetName()
    {
        return "FireworkGrandFinale";
    }

    public override string GetPickupModelName()
    {
        return "GrandFinale.prefab";
    }
    
    public override float GetModelScale()
    {
        return 3f;
    }

    public override string GetPickupIconName()
    {
        return "GrandFinale.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.Tier3;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist };
    }
    
    public override string GetItemName()
    {
        return "Grand Finale";
    }

    public override string GetItemPickup()
    {
        return "Launch a grand finale firework after killing 15 enemies.";
    }

    public override string GetItemDescription()
    {
        return "";  //$"Every <style=cIsDamage>{fireworkInterval.Value} seconds</style>, all non-player allies fire <style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style>";
    }

    public override string GetItemLore()
    {
        return "Ayo what we do with all these fireworks?! *END TRANSMISSION*";
    }

    public override void Init(AssetBundle bundle)
    {
        base.Init(bundle);

        projectilePrefab = bundle.LoadAsset<GameObject>("Assets/ImportModels/GrandFinaleProjectile.prefab")
            .InstantiateClone("GrandFinaleProjectile");
        projectilePrefab.layer = LayerMask.NameToLayer("Projectile");
        projectilePrefab.AddComponent<NetworkIdentity>();
        var tf = projectilePrefab.AddComponent<TeamFilter>();
        tf.teamIndex = TeamIndex.Monster;
        var pc = projectilePrefab.AddComponent<ProjectileController>();
        pc.procCoefficient = 1f;
        pc.allowPrediction = true;
        projectilePrefab.AddComponent<ProjectileDamage>();
        var pie = projectilePrefab.AddComponent<ProjectileImpactExplosion>();
        pie.lifetime = 99f;
        pie.destroyOnEnemy = true;
        pie.destroyOnWorld = true;
        pie.impactOnWorld = true;
        pie.alive = true;
        pie.explodeOnLifeTimeExpiration = true;
        pie.blastDamageCoefficient = 1f;
        pie.blastProcCoefficient = 1f;
        pie.blastRadius = fireworkExplosionSize.Value;
        pie.transformSpace = ProjectileImpactExplosion.TransformSpace.World;
        pie.dotDamageMultiplier = 1f;
        pie.canRejectForce = true;
        pie.dotIndex = DotController.DotIndex.None;
        pie.falloffModel = BlastAttack.FalloffModel.None;
        //pie.impactEffect = "FireworkExplosion2";
        var missileController = projectilePrefab.AddComponent<MissileController>();
        missileController.maxVelocity = 25f;
        missileController.maxSeekDistance = 100f;
        missileController.acceleration = 2f;
        missileController.rollVelocity = 3f;
        missileController.giveupTimer = 8f;
        missileController.deathTimer = 16f;
        missileController.turbulence = 8f;
        var rb = projectilePrefab.GetComponent<Rigidbody>();
        rb.useGravity = false;
        var qpid = projectilePrefab.AddComponent<QuaternionPID>();
        qpid.gain = 20;
        qpid.PID = new Vector3(5, 1, 0);
        projectilePrefab.AddComponent<ProjectileTargetComponent>();
    }
    
    private void RefreshBuffCount(CharacterBody body)
    {
        /*var coatCount = body.inventory.GetItemCount(RoR2.Items.ImmuneToDebuffBehavior.GetItemDef());
        body.SetBuffCount(DLC1Content.Buffs.ImmuneToDebuffReady.buffIndex, coatCount);*/
        // TODO custom debuff to indicate time until
    }
        
    public void FixCounts()
    {
        // Fix Ben's Raincoat stack not updating in the buff bar
        var instances = PlayerCharacterMasterController.instances;
        if (instances == null)
            return;
        
        foreach (PlayerCharacterMasterController playerCharacterMaster in instances)
        {
            var body = playerCharacterMaster.master.GetBody();
            if (body == null)
                continue;
            RefreshBuffCount(body);
        }
    }

    public override void FixedUpdate()
    {
        FixCounts();
    }
    
    public override void AddHooks()
    {
        // Implement fireworks on kill
        GlobalEventManager.onCharacterDeathGlobal += (report) =>
        {
            if (!report.attacker || !report.attackerBody)
                return;

            var attackerCharacterBody = report.attackerBody;
            if (attackerCharacterBody.inventory && attackerCharacterBody.teamComponent.teamIndex != report.victim.body.teamComponent.teamIndex)
            {
                var count = attackerCharacterBody.inventory.GetItemCount(Item);
                if (count > 0)
                {
                    ProjectileManager.instance.FireProjectile(projectilePrefab, 
                        attackerCharacterBody.corePosition + Vector3.up * attackerCharacterBody.radius, 
                        Quaternion.LookRotation(Vector3.up), attackerCharacterBody.gameObject, 
                        fireworkDamage.Value * attackerCharacterBody.baseDamage, 50f, 
                        attackerCharacterBody.RollCrit());
                }
            }
        };
    }
}