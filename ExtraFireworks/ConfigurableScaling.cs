using BepInEx.Configuration;
using MiscFixes.Modules;
using UnityEngine;

namespace ExtraFireworks
{
    public abstract class ConfigurableScaling
    {
        private readonly ConfigEntry<float> starting;
        private readonly ConfigEntry<float> scale;

        public float Base => starting.Value;
        public float Scaling => scale.Value;

        public abstract string GetBaseDescription();
        public abstract string GetScalingDescription();
        public abstract float RawValue(int stacks);

        public ConfigurableScaling(string configSection, float defaultStart, float defaultScale)
        {
            starting = ExtraFireworks.instance.Config.BindOptionSlider(
                configSection,
                "BaseValue",
                GetBaseDescription(),
                defaultStart,
                0f, 100f);

            scale = ExtraFireworks.instance.Config.BindOptionSlider(
                configSection,
                "ScaleAdditionalStacks",
                GetScalingDescription(),
                defaultScale,
                0f, 100f);
        }

        public float GetValue(int stacks) => stacks <= 0 ? 0 : RawValue(stacks);
        public int GetValueInt(int stacks) => Mathf.RoundToInt(GetValue(stacks));
    }

    public class ConfigurableLinearScaling(string configSection, float defaultStart, float defaultScale) 
        : ConfigurableScaling(configSection, defaultStart, defaultScale)
    {
        public override string GetBaseDescription() => "Base scaling value";

        public override string GetScalingDescription() => "Additional stacks scaling value";

        public override float RawValue(int stacks) => Base + Scaling * (stacks - 1);
    }

    public class ConfigurableHyperbolicScaling(string configSection, float defaultStart, float defaultScale)
        : ConfigurableScaling(configSection, defaultStart, defaultScale)
    {
        public override string GetBaseDescription() => "Max-cap ceiling value";
        public override string GetScalingDescription() => "Hyperbolic scaling constant";

        public override float RawValue(int stacks) => Base * (1 - (1f / (1f + (Scaling * stacks))));
    }
}