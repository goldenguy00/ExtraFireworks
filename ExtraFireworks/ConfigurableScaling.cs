using BepInEx.Configuration;
using UnityEngine;

namespace ExtraFireworks;

public abstract class ConfigurableScaling
{
    private ConfigEntry<float> starting;
    private ConfigEntry<float> scale;
    
    public float Base
    {
        get => starting.Value;
    }

    public float Scaling
    {
        get => scale.Value;
    }

    public abstract string GetBaseDescription();
    public abstract string GetScalingDescription();

    public abstract float RawValue(int stacks);
    
    public ConfigurableScaling(ConfigFile config, string prefix, string configSection, float defaultStart, float defaultScale)
    {
        // if prefix == null: prefix = ""
        prefix ??= "";
        
        starting = config.Bind(configSection, "BaseValue", defaultStart, GetBaseDescription());
        scale = config.Bind(configSection, "ScaleAdditionalStacks", defaultScale, GetScalingDescription());
    }
    
    public float GetValue(int stacks)
    {
        if (stacks <= 0)
            return 0;

        return RawValue(stacks);
    }

    public int GetValueInt(int stacks)
    {
        return Mathf.RoundToInt(GetValue(stacks));
    }
}