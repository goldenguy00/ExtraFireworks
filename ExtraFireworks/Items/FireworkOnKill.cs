using HG;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkOnKill : ItemBase<FireworkOnKill>
    {
        internal readonly ConfigurableLinearScaling scaler;

        public FireworkOnKill() : base()
        {
            scaler = new ConfigurableLinearScaling(ConfigSection, 2, 2);
        }

        public override string UniqueName => "FireworkOnKill";

        public override string PickupModelName => "Will-o-the-Firework.prefab";

        public override string PickupIconName => "BottledFireworks.png";

        public override Vector3? ModelScale => Vector3.one * 1.2f;

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Will-o'-the-Firework";

        public override string ItemPickupDescription => "Spawn fireworks on kill";

        public override string ItemDescription =>
            $"On <style=cIsDamage>killing an enemy</style>, release a " +
            $"<style=cIsDamage>barrage of {scaler.Base}</style> " +
            $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style> for " +
            $"<style=cIsDamage>300%</style> base damage each.";

        public override string ItemLore => "Revolutionary design.";

#pragma warning disable CS0618 // Type or member is obsolete
        public override void AdjustPickupModel()
        {
            var prefab = this.Item?.pickupModelPrefab;
            if (prefab)
            {
                prefab.transform.Find("GlassJarLid").SetAsFirstSibling();

                var jarPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/PickupWilloWisp.prefab").WaitForCompletion();
                var glass = jarPrefab.transform.Find("mdlGlassJar").Find("GlassJar").GetComponent<Renderer>();
                var fireGlass = prefab.transform.Find("GlassJar").GetComponent<Renderer>();

                fireGlass.material = glass.material;
                fireGlass.materials = glass.materials;

                if (this.ModelScale.HasValue)
                    prefab.transform.localScale = this.ModelScale.Value;

                var mdlParams = prefab.EnsureComponent<ModelPanelParameters>();

                if (!mdlParams.focusPointTransform)
                {
                    mdlParams.focusPointTransform = new GameObject("FocusPoint").transform;
                    mdlParams.focusPointTransform.SetParent(this.Item.pickupModelPrefab.transform);
                }
                if (!mdlParams.cameraPositionTransform)
                {
                    mdlParams.cameraPositionTransform = new GameObject("CameraPosition").transform;
                    mdlParams.cameraPositionTransform.SetParent(this.Item.pickupModelPrefab.transform);
                }
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public class FireworkOnKillBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => FireworkOnKill.Instance.Item;

        public void OnKilledOtherServer(DamageReport report)
        {
            if (!report.victimBody)
                return;

            var transform = report.victimBody.coreTransform ? report.victimBody.coreTransform : report.victimBody.transform;
            ExtraFireworks.SpawnFireworks(transform, report.attackerBody, FireworkOnKill.Instance.scaler.GetValueInt(base.stack), false);
        }
    }
}