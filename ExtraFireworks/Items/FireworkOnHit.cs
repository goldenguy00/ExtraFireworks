using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkOnHit : ItemBase<FireworkOnHit>
    {
        private readonly ConfigurableLinearScaling scaler;
        private readonly ConfigEntry<int> numFireworks;

        private const float MAX_FIREWORK_HEIGHT = 50f;

        public FireworkOnHit() : base()
        {
            numFireworks = PluginConfig.config.Bind(ConfigSection, "FireworksPerHit", 1, "Number of fireworks per hit");
            scaler = new ConfigurableLinearScaling(ConfigSection, 10, 10);
        }

        public override string UniqueName => "FireworkOnHit";

        public override string PickupModelName => "Firework Dagger.prefab";

        public override float ModelScale => 0.15f;

        public override string PickupIconName => "FireworkDagger.png";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Firework Dagger";

        public override string ItemPickupDescription => "Chance to fire fireworks on hit";

        public override string ItemDescription
        {
            get
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
        }

        public override string ItemLore => "You got stabbed by a firework and is kill.";

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