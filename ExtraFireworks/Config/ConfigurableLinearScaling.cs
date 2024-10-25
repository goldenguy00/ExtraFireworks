namespace ExtraFireworks.Config
{
    public class ConfigurableLinearScaling(string configSection, float defaultStart, float defaultScale) : ConfigurableScaling(configSection, defaultStart, defaultScale)
    {
        public override string GetBaseDescription() => "Base scaling value";

        public override string GetScalingDescription() => "Additional stacks scaling value";

        public override float RawValue(int stacks) => Base + Scaling * (stacks - 1);
    }
}