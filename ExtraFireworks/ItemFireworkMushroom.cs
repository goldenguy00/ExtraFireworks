using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtraFireworks;

public class ItemFireworkMushroom : FireworkItem
{
    private ConfigurableHyperbolicScaling scaler;
    
    private Dictionary<CharacterBody, GameObject> mushroomFireworkGameObject;
    private Dictionary<CharacterBody, float> fungusTimers;
    private GameObject mushroomFireworkPrefab;
    
    public ItemFireworkMushroom(ExtraFireworks plugin, ConfigFile config) : base(plugin, config)
    {
        scaler = new ConfigurableHyperbolicScaling(config, "", GetConfigSection(), 1, 0.1f);
        
        // Loading fungus shit in
        mushroomFireworkGameObject = new Dictionary<CharacterBody, GameObject>();
        fungusTimers = new Dictionary<CharacterBody, float>();
        mushroomFireworkPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MushroomWard");    
    }

    public override string GetName()
    {
        return "FireworkMushroom";
    }

    public override string GetPickupModelName()
    {
        return "Fungus.prefab";
    }
    
    public override float GetModelScale()
    {
        return 0.75f;
    }
    
    public override string GetPickupIconName()
    {
        return "Fungus.png";
    }

    public override ItemTier GetTier()
    {
        return ItemTier.Tier1;
    }
    
    public override ItemTag[] GetTags()
    {
        return new[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist };
    }
    public override string GetItemName()
    {
        return "Fungus";
    }

    public override string GetItemPickup()
    {
        return "Become a firework launcher when you stand still.";
    }

    public override string GetItemDescription()
    {
        return
            $"After <style=cIsUtility>standing still</style> for <style=cIsUtility>1 second</style>, shoot fireworks " +
            $"at <style=cIsDamage>{scaler.GetValue(1) * 100:0}%</style> " +
            $"<style=cStack>(+{(scaler.GetValue(2) - scaler.GetValue(1)) * 100:0} per stack)</style> speed " +
            $"<style=cStack>(hyperbolic up to 100%)</style> that deal <style=cIsDamage>300%</style> base damage.";
    }

    public override string GetItemLore()
    {
        return "A fun arts and crafts project.";
    }

    public override void AddHooks()
    {
        On.RoR2.CharacterBody.OnSkillActivated += (orig, self, skill) =>
        {
            if (!self.inventory || skill == null)
                goto end;

            var stack = self.inventory.GetItemCount(Item);
            if (stack > 0 && skill != self.skillLocator?.primary)
                ExtraFireworks.FireFireworks(self, scaler.GetValueInt(stack));
                
            end:
            orig(self, skill);
        };
    }

    public override void FixedUpdate()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        
        var team = TeamComponent.GetTeamMembers(TeamIndex.Player);
        foreach (var member in team)
        {
            if (!member.body)
                continue;

            var body = member.body;
            if (!body.inventory)
                continue;
            
            var stack = body.inventory.GetItemCount(Item);
            bool flag = stack > 0 && body.GetNotMoving();
            
            // Handle creating the fungus effect
            if (mushroomFireworkGameObject.ContainsKey(body) != flag)
            {
                if (flag)
                {
                    var go = ExtraFireworks.Instantiate(mushroomFireworkPrefab, body.footPosition, Quaternion.identity);
                    var healingWard = go.GetComponent<HealingWard>();
                    NetworkServer.Spawn(go);

                    if (healingWard)
                    {
                        healingWard.healFraction = 0f;
                        healingWard.healPoints = 0f;
                        // 1/2 of bungus size
                        healingWard.Networkradius = (body.radius + 1.5f * 2) / 3f;
                    }
                    
                    go.transform.parent = body.gameObject?.transform;
                    mushroomFireworkGameObject[body] = go;
                }
                else
                {
                    var go = mushroomFireworkGameObject[body];
                    ExtraFireworks.Destroy(go);
                    mushroomFireworkGameObject.Remove(body);
                }
            }
            
            // Handle spawning in fireworks
            if (fungusTimers.ContainsKey(body))
                fungusTimers[body] -= Time.fixedDeltaTime;
            
            if (flag && (!fungusTimers.ContainsKey(body) || fungusTimers[body] <= 0))
            {
                var launcher = ExtraFireworks.FireFireworks(body, 1);
                launcher.launchInterval /= (1 - 1 / (1f + 0.05f * stack));
                
                fungusTimers[body] = launcher.launchInterval;
            }
        }
    }
}