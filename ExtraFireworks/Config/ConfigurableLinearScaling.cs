namespace ExtraFireworks.Config
{
    public class ConfigurableLinearScaling(string prefix, string configSection, float defaultStart, float defaultScale) : ConfigurableScaling(prefix, configSection, defaultStart, defaultScale)
    {
        public override string GetBaseDescription() => "Base scaling value";

        public override string GetScalingDescription() => "Additional stacks scaling value";

        public override float RawValue(int stacks) => Base + Scaling * (stacks - 1);
    }
}