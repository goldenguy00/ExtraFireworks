using BepInEx.Configuration;
using UnityEngine;

namespace ExtraFireworks;

public class ConfigurableLinearScaling : ConfigurableScaling
{
    public ConfigurableLinearScaling(ConfigFile config, string prefix, string configSection, float defaultStart, float defaultScale) : base(config, prefix, configSection, defaultStart, defaultScale)
    {
    }

    public override string GetBaseDescription()
    {
        return "Base scaling value";
    }

    public override string GetScalingDescription()
    {
        return "Additional stacks scaling value";
    }

    public override float RawValue(int stacks)
    {
        return Base + Scaling * (stacks - 1);
    }
}