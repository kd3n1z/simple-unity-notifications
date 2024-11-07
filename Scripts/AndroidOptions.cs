using JetBrains.Annotations;
using UnityEngine;

namespace Sun {
    /// <summary>
    /// Represents options for Android-specific notification settings.
    /// </summary>
    public struct AndroidOptions {
        /// <summary>
        /// Gets or sets the name of the small icon for the notification.
        /// Can be <c>null</c>.
        /// </summary>
        [CanBeNull] public string SmallIconName;

        /// <summary>
        /// Gets or sets the name of the large icon for the notification.
        /// Can be <c>null</c>.
        /// </summary>
        [CanBeNull] public string LargeIconName;

        /// <summary>
        /// Gets or sets the accent color for the notification.
        /// Can be <c>null</c>.
        /// </summary>
        public Color? AccentColor;

        /// <summary>
        /// Gets the default AndroidOptions with empty icon names and a clear accent color.
        /// </summary>
        public static AndroidOptions Default => new AndroidOptions {
            SmallIconName = "",
            LargeIconName = "",
            AccentColor = Color.clear
        };

        /// <summary>
        /// Overrides the <see cref="AndroidOptions"/> with the specified options, if provided.
        /// </summary>
        /// <param name="basic">The <see cref="AndroidOptions"/> to be overridden.</param>
        /// <param name="overrides">The <see cref="AndroidOptions"/> to apply as overrides. Can be <c>null</c>.</param>
        /// <returns>A new <see cref="AndroidOptions"/> instance with applied overrides.</returns>
        public static AndroidOptions Override(AndroidOptions basic, AndroidOptions? overrides) {
            if (!overrides.HasValue) {
                return basic;
            }

            AndroidOptions result = basic;
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