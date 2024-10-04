using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks;

public class ItemFireworkDaisy : FireworkItem
{
    private ConfigEntry<int> fireworksPerWave;
    private Dictionary<HoldoutZoneController, float> lastCharge;

    public ItemFireworkDaisy(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworksPerWave = config.Bind(GetConfigSection(), "FireworksPerWave", 40,
            "Number of fireworks per firework daisy wave");
        
        lastCharge = new Dictionary<HoldoutZoneController, float>();
    }

    public override string GetName()
    {
        return "FireworkDaisy";
    }

    public override string GetPickupModelName()
    {
        return "Firework Daisy.prefab";
    }
    
    public override float GetModelScale()
    {
        return 1.5f;
    }

    public override string GetPickupIconName()
    {
        return "FireworkDaisy.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.Tier2;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.HoldoutZoneRelated, ItemTag.AIBlacklist };
    }

    public override string GetItemName()
    {
        return "Firework Daisy";
    }

    public override string GetItemPickup()
    {
        return "Periodically releases waves of fireworks during the teleporter event";
    }

    public override string GetItemDescription()
    {
        return $"Release a barrage of fireworks during the Teleporter event, dealing <style=cIsDamage>{fireworksPerWave.Value}x300% base damage</style>. Occurs 2 <style=cStack>(+1 per stack)</style> times.";
    }

    public override string GetItemLore()
    {
        return "A lepton daisy with a firework jammed in it.";
    }

    public override void AddHooks()
    {
        On.RoR2.HoldoutZoneController.OnDisable += (orig, self) =>
        {
            lastCharge.Remove(self);
            orig(self);
        };

        On.RoR2.HoldoutZoneController.Update += (orig, self) =>
        {
            lastCharge[self] = self.charge;

            orig(self);

            if (!NetworkServer.active)
                return;

            var last = lastCharge[self];
            for (TeamIndex idx = 0; idx < TeamIndex.Count; idx++)
            {
                var count = Util.GetItemCountForTeam(idx, Item.itemIndex, false);
                if (count <= 0)
                    continue;

                var nextCharge = GetNextFireworkCharge(last, count);

                // Get a random body to assign the fireworks to
                var teamMembers = TeamComponent.GetTeamMembers(idx);
                CharacterBody body = null;
                while (body == null)
                {
                    var randMember = teamMembers[Random.Range(0, teamMembers.Count)];
                    body = randMember.body;
                }

                if (self.charge >= nextCharge && last < nextCharge)
                    ExtraFireworks.SpawnFireworks(self.healingNovaRoot ?? self.transform, body, fireworksPerWave.Value);
            }
        };
    }
    
    private static float GetNextFireworkCharge(float charge, int stacks)
    {
        var frac = 1f / (1 + stacks);
        var num = charge / frac;
        return Mathf.Ceil(num) * frac;
    }
}