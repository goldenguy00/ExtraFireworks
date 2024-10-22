using BepInEx.Configuration;
using ExtraFireworks.Config;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks.Items
{
    public class ItemFireworkAbility : FireworkItem<ItemFireworkAbility>
    {
        internal static ConfigurableLinearScaling scaler;
        internal static ConfigEntry<bool> noSkillRestriction;

        public ItemFireworkAbility() : base()
        {
            scaler = new ConfigurableLinearScaling("", GetConfigSection(), 1, 1);
            noSkillRestriction = PluginConfig.config.Bind(GetConfigSection(), "PrimaryAbilityFireworks", false,
                "Whether abilities without a cooldown should spawn fireworks... be wary of brokenness, especially on Commando and Railgunner");
        }

        public override string GetName() => "FireworkAbility";

        public override string GetPickupModelName() => "Firework-Stuffed Head.prefab";

        public override float GetModelScale() => 1.1f;

        public override string GetPickupIconName() => "FireworkStuffedHead.png";

        public override ItemTier GetTier() => ItemTier.Tier2;

        public override ItemTag[] GetTags() => [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];

        public override string GetItemName() => "Firework-Stuffed Head";

        public override string GetItemPickup() => "Using abilities now spawns fireworks";

        public override string GetItemDescription()
        {
            return $"Using a <style=cIsUtility>non-primary skill</style> fires <style=cIsDamage>{scaler.Base}</style> " +
                   $"<style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>firework</style> for " +
                   $"<style=cIsDamage>300%</style> base damage.";
        }

        public override string GetItemLore() => "Holy shit it's a head with fireworks sticking out of it";

        public override void AddHooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += this.CharacterBody_OnInventoryChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active)
                self.AddItemBehavior<FireworkAbilityBehaviour>(self.inventory.GetItemCount(this.Item));
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
                if (ItemFireworkAbility.noSkillRestriction.Value || (skill.baseRechargeInterval >= 1f - Mathf.Epsilon && skill.skillDef.stockToConsume > 0))
                    ExtraFireworks.FireFireworks(body, ItemFireworkAbility.scaler.GetValueInt(stack));
            }
        }
    }
}