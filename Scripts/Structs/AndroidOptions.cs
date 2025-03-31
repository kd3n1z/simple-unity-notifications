using JetBrains.Annotations;
using UnityEngine;

namespace Sun {
    /// <summary>
    /// Android-specific notification settings, including icons and accent color.
    /// </summary>
    public struct AndroidOptions {
        [CanBeNull] public string SmallIconName;
        [CanBeNull] public string LargeIconName;
        public Color? AccentColor;

        /// <summary>
        /// Default Android notification options.
        /// </summary>
        public static readonly AndroidOptions Default = new AndroidOptions {
            SmallIconName = "",
            LargeIconName = "",
            AccentColor = Color.clear
        };

        /// <summary>
        /// Overrides existing Android options with provided values.
        /// </summary>
        /// <param name="overrides">Optional new values to override the existing ones.</param>
        /// <returns>A new AndroidOptions instance with applied overrides.</returns>
        public AndroidOptions Override(AndroidOptions? overrides) {
            AndroidOptions result = this;

            if (overrides.HasValue) {
                AndroidOptions overridesValue = overrides.Value;

                if (overridesValue.AccentColor.HasValue) {
                    result.AccentColor = overridesValue.AccentColor.Value;
                }

                if (overridesValue.SmallIconName != null) {
                    result.SmallIconName = overridesValue.SmallIconName;
                }

                if (overridesValue.LargeIconName != null) {
                    result.LargeIconName = overridesValue.LargeIconName;
                }
            }

            return result;
        }
    }
}