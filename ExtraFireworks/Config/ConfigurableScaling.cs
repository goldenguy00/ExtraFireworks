using BepInEx.Configuration;
using UnityEngine;

namespace ExtraFireworks.Config
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
            starting = PluginConfig.BindOptionSlider(
                configSection,
                "BaseValue",
                defaultStart,
                GetBaseDescription(),
                0, 100);

            scale = PluginConfig.BindOptionSlider(
                configSection,
                "ScaleAdditionalStacks",
                defaultScale,
                GetScalingDescription(),
                0, 100);
        }

        public float GetValue(int stacks) => stacks <= 0 ? 0 : RawValue(stacks);
        public int GetValueInt(int stacks) => Mathf.RoundToInt(GetValue(stacks));
    }
}