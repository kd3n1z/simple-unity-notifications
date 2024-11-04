#if UNITY_IOS

using System;
using System.Collections;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Sun {
    // ReSharper disable once InconsistentNaming
    internal static class iOSNotifications {
        internal static IEnumerator RequestAuthorization() {
            AuthorizationRequest request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);
            yield return new WaitWhile(() => !request.IsFinished);
        }

        internal static void ScheduleNotification(string title, string body, TimeSpan timeInterval) {
            iOSNotificationTimeIntervalTrigger timeTrigger = new iOSNotificationTimeIntervalTrigger() {
                TimeInterval = timeInterval,
                Repeats = false
            };

            iOSNotification notification = new iOSNotification() {
                Title = title,
                Body = body,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Badge | PresentationOption.Sound,
                CategoryIdentifier = "default_category",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };
        }

        public static void ClearScheduledNotifications() => iOSNotificationCenter.RemoveAllScheduledNotifications();
    }
}

#endif