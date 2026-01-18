using BepInEx.Configuration;
using MiscFixes.Modules;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace ExtraFireworks.Items
{
    public class FireworkAbility : ItemBase<FireworkAbility>
    {
        internal static ConfigurableLinearScaling scaler;
        internal static ConfigEntry<bool> noSkillRestriction;

        public FireworkAbility() : base()
        {
            scaler = new ConfigurableLinearScaling(ConfigSection, 1, 1);

            noSkillRestriction = ExtraFireworks.instance.Config.BindOption(
                ConfigSection,
                "PrimaryAbilityFireworks",
                "Whether abilities without a cooldown should spawn fireworks... be wary of brokenness, especially on Commando and Railgunner",
                false);
        }

        public override string UniqueName => "FireworkAbility";

        public override string PickupModelName => "Firework-Stuffed Head.prefab";

        public override string PickupIconName => "FireworkStuffedHead.png";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] Tags => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string ItemName => "Firework-Stuffed Head";

        public override string ItemPickupDescription => "Using abilities now spawns fireworks";

        public override string ItemDescription =>
            $"Using a <style=cIsUtility>non-primary skill</style> fires <style=cIsDamage>{scaler.Base}</style> " +
            $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>firework</style> for " +
            $"<style=cIsDamage>300%</style> base damage.";

        public override string ItemLore => "Holy shit it's a head with fireworks sticking out of it";

        public override void Init(AssetBundle bundle)
        {
            base.Init(bundle);
        }
    }

    public class FireworkAbilityBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => FireworkAbility.Instance?.Item;

        private void Start()
        {
            if (!ReferenceEquals(body, null))
                body.onSkillActivatedServer += Body_onSkillActivatedServer;
        }
        private void OnDestroy()
        {
            if (!ReferenceEquals(body, null))
                body.onSkillActivatedServer -= Body_onSkillActivatedServer;
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            if (skill?.skillDef)
            {
                if (FireworkAbility.noSkillRestriction.Value || (skill.baseRechargeInterval >= 1f - Mathf.Epsilon && skill.skillDef.stockToConsume > 0))
                    ExtraFireworks.FireFireworks(body, FireworkAbility.scaler.GetValueInt(stack));
            }
        }
    }
}