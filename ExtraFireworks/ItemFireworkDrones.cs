using System.Collections;

namespace ExtraFireworks;

using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

using Random = System.Random;

public class ItemFireworkDrones : FireworkItem
{
    private ConfigEntry<float> fireworkInterval;
    private ConfigurableLinearScaling scaler;

    private Dictionary<CharacterBody, float> timers;
    
    public ItemFireworkDrones(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        fireworkInterval = config.Bind(GetConfigSection(), "FireworksInterval", 4f,
            "Number of seconds between bursts of fireworks");
        scaler = new ConfigurableLinearScaling(config, "", GetConfigSection(), 4, 2);
        timers = new Dictionary<CharacterBody, float>();
    }

    public override string GetName()
    {
        return "FireworkDrones";
    }

    public override string GetPickupModelName()
    {
        return "SpareFireworks.prefab";
    }

    public override string GetPickupIconName()
    {
        return "SpareFireworks.png";
    }

    public override ItemTiers GetTier()
    {
        return ItemTiers.Red;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage };
    }


    public override float GetModelScale()
    {
        return .25f;
    }

    public override string GetItemName()
    {
        return "Spare Fireworks";
    }

    public override string GetItemPickup()
    {
        return "All drones now shoot fireworks";
    }

    public override string GetItemDescription()
    {
        return $"Every <style=cIsDamage>{fireworkInterval.Value} seconds</style>, all non-player allies fire <style=cIsDamage>{scaler.Base}</style> <style=cStack>(+{scaler.Scaling} per stack)</style> <style=cIsDamage>fireworks</style>";
    }

    public override string GetItemLore()
    {
        return "Ayo what we do with all these fireworks?! *END TRANSMISSION*";
    }

    public override void FixedUpdate()
    {
        if (!NetworkServer.active)
            return;

        for (TeamIndex idx = 0; idx < TeamIndex.Count; idx++)
        {
            var count = Util.GetItemCountForTeam(idx, Item.itemIndex, true);
            if (count <= 0)
                continue;
                    
            var teamMembers = TeamComponent.GetTeamMembers(idx);
            foreach (var member in teamMembers)
            {
                CharacterBody body = member.body;
                if (body && body.master)
                {
                    MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                    if (minionGroup != null)
                    {
                        foreach (MinionOwnership minionOwnership in minionGroup.members)
                        {
                            if (minionOwnership)
                            {
                                CharacterMaster component = minionOwnership.GetComponent<CharacterMaster>();
                                if (component && component.inventory)
                                {
                                    CharacterBody body2 = component.GetBody();

                                    if (!body2)
                                        continue;
                                    
                                    if (!timers.ContainsKey(body2))
                                        timers[body2] = RandomStartDelta();

                                    var t = timers[body2];
                                    t -= Time.fixedDeltaTime;
                                    
                                    if (t <= 0)
                                    {
                                        ExtraFireworks.SpawnFireworks(body2.coreTransform, body, 2 + 2 * count);
                                        t = fireworkInterval.Value;
                                    }
                                    
                                    timers[body2] = t;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private float RandomStartDelta()
    {
        return UnityEngine.Random.value * fireworkInterval.Value;
    }
}