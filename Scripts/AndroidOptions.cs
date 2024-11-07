using JetBrains.Annotations;
using UnityEngine;

namespace Sun {
    public struct AndroidOptions {
        [CanBeNull] public string SmallIconName;
        [CanBeNull] public string LargeIconName;
        public Color? AccentColor;

        public static AndroidOptions Default => new AndroidOptions {
            SmallIconName = "",
            LargeIconName = "",
            AccentColor = Color.clear
        };

        public static AndroidOptions Override(AndroidOptions defaults, AndroidOptions? overrides) {
            if (!overrides.HasValue) {
                return defaults;
            }

            AndroidOptions result = defaults;
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

            return result;
        }
    }
}