namespace ExtraFireworks.Config
{
    public class ConfigurableHyperbolicScaling(string configSection, float defaultStart, float defaultScale) : ConfigurableScaling(configSection, defaultStart, defaultScale)
    {
        public override string GetBaseDescription() => "Max-cap ceiling value";
        public override string GetScalingDescription() => "Hyperbolic scaling constant";

        public override float RawValue(int stacks) => Base * (1 - (1f / (1f + (Scaling * stacks))));
    }
}