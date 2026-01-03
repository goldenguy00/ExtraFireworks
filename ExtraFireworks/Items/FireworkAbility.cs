using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class FireworkAbility : ItemBase<FireworkAbility>
    {
        internal static ConfigurableLinearScaling scaler;
        internal static ConfigEntry<bool> noSkillRestriction;

        public FireworkAbility() : base()
        {
            scaler = new ConfigurableLinearScaling(ConfigSection, 1, 1);

            noSkillRestriction = PluginConfig.BindOption(
                ConfigSection,
                "PrimaryAbilityFireworks", 
                false,
                "Whether abilities without a cooldown should spawn fireworks... be wary of brokenness, especially on Commando and Railgunner");
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

        public override void AddHooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += this.CharacterBody_OnInventoryChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active)
                self.AddItemBehavior<FireworkAbilityBehaviour>(self.inventory.GetItemCountEffective(this.Item));
        }
    }

    public class FireworkAbilityBehaviour : CharacterBody.ItemBehavior
    {
        private void Awake()
        {
            this.enabled = false;
        }

        private void OnEnable()
        {
            body.onSkillActivatedServer += Body_onSkillActivatedServer;
        }
        private void OnDisable()
        {
            if (body)
                body.onSkillActivatedServer -= Body_onSkillActivatedServer;
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            if (this.stack > 0 && skill && skill.skillDef)
            {
                if (FireworkAbility.noSkillRestriction.Value || (skill.baseRechargeInterval >= 1f - Mathf.Epsilon && skill.skillDef.stockToConsume > 0))
                    ExtraFireworks.FireFireworks(body, FireworkAbility.scaler.GetValueInt(stack));
            }
        }
    }
}