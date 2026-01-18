using BepInEx.Configuration;
using MiscFixes.Modules;
using RoR2;
using RoR2.Items;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace ExtraFireworks.Items
{
    public class FireworkOnHit : ItemBase<FireworkOnHit>
    {
        internal readonly ConfigurableLinearScaling scaler;
        internal readonly ConfigEntry<int> numFireworks;

        internal const float MAX_FIREWORK_HEIGHT = 50f;

        public FireworkOnHit() : base()
        {
            numFireworks = ExtraFireworks.instance.Config.BindOptionSlider(ConfigSection,
                "FireworksPerHit",
                "Number of fireworks per hit",
                2,
                1, 10);

            scaler = new ConfigurableLinearScaling(ConfigSection, 10, 10);
        }

        public override string UniqueName => "FireworkOnHit";

        public override string PickupModelName => "Firework Dagger.prefab";

        public override string PickupIconName => "FireworkDagger.png";

        public override Vector3? ModelScale => Vector3.one;

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

#pragma warning disable CS0618 // Type or member is obsolete
        public override void AdjustPickupModel()
        {
            base.AdjustPickupModel();

            var prefab = this.Item?.pickupModelPrefab;
            if (prefab)
            {
                var mdl = prefab.transform.Find("Firework Dagger");
                mdl.localPosition = new Vector3(0f, 2f, 0f);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public class FireworkOnHitBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => FireworkOnHit.Instance.Item;

        public void OnDamageDealtServer(DamageReport report)
        {
            if (!body)
                return;

            var damageInfo = report.damageInfo;
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected)
                return;

            // Check to make sure fireworks don't proc themselves
            // Fireworks can't proc themselves even if outside the proc chain
            if (damageInfo.procChainMask.HasProc(ProcType.MicroMissile) || (damageInfo.inflictor && damageInfo.inflictor.GetComponent<MissileController>()))
                return;

            if (Util.CheckRoll(FireworkOnHit.Instance.scaler.GetValue(base.stack) * damageInfo.procCoefficient, body.master))
            {
                //var fireworkPos = victim.transform;
                var victimBody = report.victimBody;

                // Try to refine fireworkPos using a raycast
                var basePos = damageInfo.position;
                if (victimBody?.mainHurtBox && Vector3.Distance(basePos, Vector3.zero) < Mathf.Epsilon)
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

                ExtraFireworks.CreateLauncher(body, bestPoint + Vector3.up * 2f, FireworkOnHit.Instance.numFireworks.Value);
                damageInfo.procChainMask.AddProc(ProcType.MicroMissile);
            }
        }
    }
}