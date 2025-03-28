using System;
using UnityEngine;

namespace Sun {
    [Serializable]
    internal class SavedNotification {
        [SerializeField] internal long fireTimestamp;
        [SerializeField] internal string title;
        [SerializeField] internal string text;

#if UNITY_ANDROID

        [SerializeField] internal string androidSmallIconName;
        [SerializeField] internal string androidLargeIconName;
        [SerializeField] internal float androidAccentColorR;
        [SerializeField] internal float androidAccentColorG;
        [SerializeField] internal float androidAccentColorB;
        [SerializeField] internal float androidAccentColorA;

#endif

        internal SavedNotification(DateTime dateTime, CommonOptions commonOptions, AndroidOptions androidOptions) {
            fireTimestamp = dateTime.ToUnixTimestamp();
            title = commonOptions.Title;
            text = commonOptions.Text;

#if UNITY_ANDROID

            androidSmallIconName = androidOptions.SmallIconName;
            androidLargeIconName = androidOptions.LargeIconName;

            if (androidOptions.AccentColor.HasValue) {
                Color accentColor = androidOptions.AccentColor.Value;

                androidAccentColorR = accentColor.r;
                androidAccentColorG = accentColor.g;
                androidAccentColorB = accentColor.b;
                androidAccentColorA = accentColor.a;
            }
#endif
        }
    }
}