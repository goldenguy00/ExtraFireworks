using BepInEx.Configuration;

namespace ExtraFireworks;

public class ConfigurableHyperbolicScaling : ConfigurableScaling
{
    public ConfigurableHyperbolicScaling(ConfigFile config, string prefix, string configSection, float defaultStart, float defaultScale) : base(config, prefix, configSection, defaultStart, defaultScale)
    {
    }
    
    public override string GetBaseDescription()
    {
        return "Max-cap ceiling value";
    }

    public override string GetScalingDescription()
    {
        return "Hyperbolic scaling constant";
    }

    public override float RawValue(int stacks)
    {
        return Base *  (1 - 1f / (1f + Scaling * stacks));
    }
}