using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace ExtraFireworks.Items
{
    public class FireworkOnHit : BaseFireworkItem<FireworkOnHit>
    {
        private readonly ConfigurableLinearScaling scaler;
        private readonly ConfigEntry<int> numFireworks;

        private const float MAX_FIREWORK_HEIGHT = 50f;

        public FireworkOnHit() : base()
        {
            numFireworks = PluginConfig.config.Bind(GetConfigSection(), "FireworksPerHit", 1, "Number of fireworks per hit");
            scaler = new ConfigurableLinearScaling("", GetConfigSection(), 10, 10);
        }

        public override string GetName() => "FireworkOnHit";

        public override string GetPickupModelName() => "Firework Dagger.prefab";

        public override float GetModelScale() => 0.15f;

        public override string GetPickupIconName() => "FireworkDagger.png";

        public override ItemTier GetTier() => ItemTier.Tier1;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string GetItemName() => "Firework Dagger";

        public override string GetItemPickup() => "Chance to fire fireworks on hit";

        public override string GetItemDescription()
        {
            var desc = $"Gain a <style=cIsDamage>{scaler.Base:0}%</style> chance " +
                       $"<style=cStack>(+{scaler.Scaling:0}% per stack)</style> <style=cIsDamage>on hit</style> to ";

            if (numFireworks.Value == 1)
                desc += "<style=cIsDamage>fire a firework</style> for <style=cIsDamage>300%</style> base damage.";
            else
                desc += $"<style=cIsDamage>fire {numFireworks.Value} fireworks</style> for <style=cIsDamage>300%</style> " +
                        $"base damage each.";

            return desc;
        }

        public override string GetItemLore() => "You got stabbed by a firework and is kill.";

        public override void AddHooks()
        {
            // Implement fireworks on hit
            On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);

                if (damageInfo?.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active)
                    return;

                // Check to make sure fireworks don't proc themselves
                // Fireworks can't proc themselves even if outside the proc chain
                if (damageInfo.procChainMask.HasProc(ProcType.MicroMissile) || (damageInfo.inflictor && damageInfo.inflictor.GetComponent<MissileController>()))
                    return;

                if (!damageInfo.attacker || !damageInfo.attacker.TryGetComponent<CharacterBody>(out var body) || !body.inventory)
                    return;

                var count = body.inventory.GetItemCount(Item);
                var basePercent = 9f;
                var scalingPercent = 9f;
                if (count > 0 && Util.CheckRoll((basePercent + (scalingPercent * (count - 1))) * damageInfo.procCoefficient, body.master))
                {
                    //var fireworkPos = victim.transform;
                    var victimBody = victim.GetComponent<CharacterBody>();

                    // Try to refine fireworkPos using a raycast
                    var basePos = damageInfo.position;
                    if (victimBody && Vector3.Distance(basePos, Vector3.zero) < Mathf.Epsilon)
                    {
                        basePos = victimBody.mainHurtBox.randomVolumePoint;
                    }

                    var bestPoint = basePos;/*
                    var bestHeight = basePos.y;

                    var hits = Physics.RaycastAll(basePos, Vector3.up, MAX_FIREWORK_HEIGHT);
                    foreach (var hit in hits)
                    {
                        var cm = hit.transform.GetComponentInParent<CharacterModel>();
                        if (!cm)
                            continue;

                        var cb = cm.body;
                        if (!cb)
                            continue;

                        if (cb != victimBody)
                            continue;

                        var hurtbox = hit.transform.GetComponentInChildren<HurtBox>();
                        if (hurtbox)
                        {
                            var col = hurtbox.collider;
                            if (!col)
                                continue;

                            var highestPoint = col.ClosestPoint(basePos + MAX_FIREWORK_HEIGHT * Vector3.up);
                            if (highestPoint.y > bestHeight)
                            {
                                bestPoint = highestPoint;
                                bestHeight = highestPoint.y;
                            }
                        }
                    }*/


                    ExtraFireworks.CreateLauncher(body, bestPoint + Vector3.up * 2f, numFireworks.Value);
                    damageInfo.procChainMask.AddProc(ProcType.MicroMissile);
                }
            };
        }
    }
}