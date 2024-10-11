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

    private BuffDef buff;
    
    private GameObject projectilePrefab;
    private GameObject grandFinaleModel;
    private GameObject projectileMdlClone;
    private Dictionary<CharacterBody, int> killCountdowns;
    
    public ItemFireworkFinale(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworkDamage = config.Bind(GetConfigSection(), "DamageCoefficient", 50f,
            "Damage of Grand Finale firework as coefficient of base damage");
        fireworkExplosionSize = config.Bind(GetConfigSection(), "ExplosionRadius", 10f,
            "Explosion radius of Grand Finale firework");
        fireworkEnemyKillcount = config.Bind(GetConfigSection(), "KillThreshold", 10,
            "Number of enemies required to proc the Grand Finale firework");

        killCountdowns = new Dictionary<CharacterBody, int>();
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
        return $"Launch a grand finale firework after killing {fireworkEnemyKillcount.Value} enemies.";
    }

    public override string GetItemDescription()
    {
        return $"<style=cIsDamage>Killing {fireworkEnemyKillcount.Value}</style> " +
               $"<style=cStack>(-50% per stack)</style> <style=cIsDamage>enemies</style> fires out a " +
               $"<style=cIsDamage>massive firework</style> that deals <style=cIsDamage>5000%</style> base damage.";
    }

    public override string GetItemLore()
    {
        return "Ayo what we do this one big ass firework?! *END TRANSMISSION*";
    }

    public override void Init(AssetBundle bundle)
    {
        base.Init(bundle);

        grandFinaleModel = bundle.LoadAsset<GameObject>("Assets/ImportModels/GrandFinaleProjectile.prefab");

        buff = ScriptableObject.CreateInstance<BuffDef>();
        buff.name = "GrandFinaleCountdown";
        buff.canStack = true;
        buff.isCooldown = false;
        buff.isDebuff = false;
        buff.buffColor = Color.red;
        buff.iconSprite = bundle.LoadAsset<Sprite>("Assets/Import/GrandFinaleBuff.png");;
        ContentAddition.AddBuffDef(buff);

        projectilePrefab = ExtraFireworks.fireworkPrefab.InstantiateClone("GrandFinaleProjectile");
        //projectilePrefab.transform.localScale *= 5;
        projectilePrefab.layer = LayerMask.NameToLayer("Projectile");
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
    
    private void RefreshBuffCount(CharacterBody body)
    {
        var finaleCount = body.inventory.GetItemCount(Item);
        if (finaleCount <= 0)
        {
            body.SetBuffCount(buff.buffIndex, 0);
            return;
        }

        if (!killCountdowns.ContainsKey(body))
            return;
        
        body.SetBuffCount(buff.buffIndex, killCountdowns[body]);
    }
        
    public void FixCounts()
    {
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

    protected void ResetKillcount(CharacterBody body, int itemCount)
    {
        killCountdowns[body] = Mathf.CeilToInt(fireworkEnemyKillcount.Value * 1.0f / Mathf.Pow(2.0f, itemCount));
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
                    if (!killCountdowns.ContainsKey(attackerCharacterBody))
                        ResetKillcount(attackerCharacterBody, count);
                    
                    var newKillcount = killCountdowns[attackerCharacterBody] - 1;
                    if (newKillcount > 0)
                    {
                        killCountdowns[attackerCharacterBody] = newKillcount;
                    }
                    else
                    {
                        ProjectileManager.instance.FireProjectile(projectilePrefab, 
                            attackerCharacterBody.corePosition + Vector3.up * attackerCharacterBody.radius, 
                            Quaternion.LookRotation(Vector3.up), attackerCharacterBody.gameObject, 
                            fireworkDamage.Value * attackerCharacterBody.baseDamage, 50f, 
                            attackerCharacterBody.RollCrit());
                        ResetKillcount(attackerCharacterBody, count);
                    }
                }
            }
        };

        On.RoR2.Projectile.ProjectileGhostController.Start += (orig, self) =>
        {
            orig(self);

            if (self.authorityTransform && self.authorityTransform.gameObject.name.StartsWith("GrandFinaleProjectile"))
            {
                self.transform.GetChild(1).localPosition = new Vector3(0, 0, -2.5f);
                self.transform.GetChild(2).localPosition = new Vector3(0, 0, -2.5f);

                var fireworkMdl = self.transform.GetChild(0);
                if (fireworkMdl.childCount == 0)
                {
                    var go = grandFinaleModel.InstantiateClone("GrandFinaleModel");
                    go.transform.parent = fireworkMdl.transform;
                    go.transform.localPosition = Vector3.zero;
                }
                else
                {
                    fireworkMdl.GetChild(0).GetComponentInChildren<MeshRenderer>().enabled = true;
                }
                fireworkMdl.GetComponent<MeshRenderer>().enabled = false;
            }
            
            // Reverts above changes, since firework prefab gets pooled
            if (self.authorityTransform && self.authorityTransform.gameObject.name.StartsWith("FireworkProjectile"))
            {
                self.transform.GetChild(1).localPosition = new Vector3(0, 0, -0.729f);
                self.transform.GetChild(2).localPosition = new Vector3(0, 0, -0.764f);

                var fireworkMdl = self.transform.GetChild(0);
                if (fireworkMdl.childCount > 0)
                {
                    fireworkMdl.GetChild(0).GetComponentInChildren<MeshRenderer>().enabled = false;
                    fireworkMdl.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        };

        On.RoR2.CharacterMaster.OnServerStageBegin += (orig, self, stage) =>
        {
            orig(self, stage);

            if (!self.playerCharacterMasterController)
                return;
            
            var body = self.playerCharacterMasterController.body;
            if (!body || !self.inventory)
                return;
            
            var itemCount = self.inventory.GetItemCount(Item);
            
            if (!killCountdowns.ContainsKey(body))
                return;
            
            if (killCountdowns.ContainsKey(body) && itemCount <= 0)
            {
                killCountdowns.Remove(body);
                return;
            }

            ResetKillcount(body, itemCount);
        };

        On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
        {
            orig(self);

            if (!self.inventory)
                return;
            
            var itemCount = self.inventory.GetItemCount(Item);
            if (itemCount <= 0 && killCountdowns.ContainsKey(self))
            {
                killCountdowns.Remove(self);
                return;
            }
                
            if (itemCount > 0 && !killCountdowns.ContainsKey(self))
                ResetKillcount(self, itemCount);
        };
    }
}