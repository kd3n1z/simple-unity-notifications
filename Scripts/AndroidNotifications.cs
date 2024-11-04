#if UNITY_ANDROID

using System;
using System.Collections;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

namespace Sun {
    internal static class AndroidNotifications {
        private const string ChannelId = "default_channel";

        internal static void RequestAuthorization() {
            const string permission = "android.permission.POST_NOTIFICATIONS";

            if (!Permission.HasUserAuthorizedPermission(permission)) {
                Permission.RequestUserPermission(permission);
            }
        }

        internal static void RegisterNotificationChannel() {
            AndroidNotificationChannel channel = new AndroidNotificationChannel {
                Id = ChannelId,
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications"
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        internal static IEnumerator RequestNotificationPermission() {
            PermissionRequest request = new PermissionRequest();
            yield return new WaitWhile(() => request.Status == PermissionStatus.RequestPending);
        }

        internal static void ScheduleNotification(string title, string text, DateTime fireTime, string smallIcon, string largeIcon) {
            AndroidNotification notification = new AndroidNotification {
                Title = title,
                Text = text,
                FireTime = fireTime,
                SmallIcon = smallIcon,
                LargeIcon = largeIcon
            };

            AndroidNotificationCenter.SendNotification(notification, ChannelId);
        }

        public static void ClearScheduledNotifications() => AndroidNotificationCenter.CancelAllScheduledNotifications();
    }
}

#endif