using BepInEx.Configuration;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks;

public class ItemFireworkOnHit : FireworkItem
{
    private ConfigurableLinearScaling scaler;
    private ConfigEntry<int> numFireworks;
    
    public ItemFireworkOnHit(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        numFireworks = config.Bind(GetConfigSection(), "FireworksPerHit", 1, "Number of fireworks per hit");
        scaler = new ConfigurableLinearScaling(config, "", GetConfigSection(), 10, 10);
    }

    public override string GetName()
    {
        return "FireworkOnHit";
    }

    public override string GetPickupModelName()
    {
        return "FireworkDagger.prefab";
    }

    public override string GetPickupIconName()
    {
        return "FireworkDagger.png";
    }

    public override ItemTiers GetTier()
    {
        return ItemTiers.White;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage };
    }

    public override float GetModelScale()
    {
        return 3f;
    }

    public override string GetItemName()
    {
        return "Firework Dagger";
    }

    public override string GetItemPickup()
    {
        return "Chance to fire fireworks on hit";
    }

    public override string GetItemDescription()
    {
        return $"Whenever you <style=cIsDamage>hit an enemy</style>, you have a <style=cIsDamage>{scaler.Base:0}%</style> <style=cStack>(+{scaler.Scaling}% per stack)</style> <style=cIsDamage>chance</style> to proc <style=cIsDamage>{numFireworks.Value} fireworks</style>.";
    }

    public override string GetItemLore()
    {
        return "You got stabbed by a firework and is kill.";
    }

    public override void AddHooks()
    {
        // Implement fireworks on hit
        On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
        {
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active)
                goto end;

            // Check to make sure fireworks don't proc themselves
            if (damageInfo.procChainMask.HasProc(ProcType.MicroMissile))
                goto end;

            // Fireworks can't proc themselves even if outside the proc chain
            if (damageInfo.inflictor && damageInfo.inflictor.GetComponent<MissileController>())
                goto end;

            if (!damageInfo.attacker)
                goto end;
            
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!body)
                goto end;

            if (!body.inventory)
                goto end;
            
            var count = body.inventory.GetItemCount(Item.itemIndex);
            if (count > 0 && Util.CheckRoll(scaler.GetValue(count) * damageInfo.procCoefficient, body.master))
            {
                ExtraFireworks.SpawnFireworks(victim.transform, body, numFireworks.Value);
                damageInfo.procChainMask.AddProc(ProcType.MicroMissile);
            }

            end:
            orig(self, damageInfo, victim);
        };
    }
}