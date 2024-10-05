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
        projectilePrefab.AddComponent<NetworkIdentity>();
        projectilePrefab.AddComponent<ProjectileController>();
        projectilePrefab.AddComponent<TeamFilter>();
        projectilePrefab.AddComponent<ProjectileDamage>();
        projectilePrefab.AddComponent<ProjectileExplosion>();
        var missileController = projectilePrefab.AddComponent<MissileController>();
        missileController.maxVelocity = 1f;
        missileController.acceleration = 1f;
        missileController.giveupTimer = 60f;
        missileController.deathTimer = 60f;
        missileController.turbulence = 1f;
        var rb = projectilePrefab.GetComponent<Rigidbody>();
        rb.useGravity = false;
        projectilePrefab.AddComponent<QuaternionPID>();
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
                    ProjectileManager.instance.FireProjectile(projectilePrefab, attackerCharacterBody.corePosition, 
                        Quaternion.LookRotation(Vector3.up), attackerCharacterBody.gameObject, 
                        fireworkDamage.Value, 50f, attackerCharacterBody.RollCrit());
                }
            }
        };
    }
}