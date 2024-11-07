using System;
using System.Collections;
using UnityEngine;

namespace Sun {
    /// <summary>
    /// The core component of Simple Unity Notifications (SUN). Manages the scheduling and removal of notifications.
    /// </summary>
    public class NotificationsManager : MonoBehaviour {
        private const string PlayerPrefsKey = "SimpleUnityNotificationsData";

        private NotificationsCollection _notifications;

        private bool _initialized;

        #region Config

        private float _debounceInterval;
        private bool _loggingEnabled;
        private string _defaultAndroidSmallIcon;
        private string _defaultAndroidLargeIcon;

        #endregion

        /// <summary>
        /// Initializes the <see cref="NotificationsManager"/> with optional parameters for default icons, debounce interval, and logging.
        /// </summary>
        /// <param name="defaultAndroidSmallIcon">The default small icon for Android notifications.</param>
        /// <param name="defaultAndroidLargeIcon">The default large icon for Android notifications.</param>
        /// <param name="debounceInterval">The time interval in seconds to debounce notification actions. Must be non-negative.</param>
        /// <param name="loggingEnabled">Specifies whether logging is enabled for notifications.</param>
        /// <exception cref="Exception">Thrown if the manager is already initialized.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the debounce interval is negative.</exception>
        public void Initialize(string defaultAndroidSmallIcon = "", string defaultAndroidLargeIcon = "", float debounceInterval = 1, bool loggingEnabled = false) {
            if (_initialized) {
                throw new Exception($"{nameof(NotificationsManager)} is already initialized");
            }

            if (debounceInterval < 0) {
                throw new ArgumentOutOfRangeException();
            }

            _debounceInterval = debounceInterval;
            _loggingEnabled = loggingEnabled;
            _defaultAndroidSmallIcon = defaultAndroidSmallIcon;
            _defaultAndroidLargeIcon = defaultAndroidLargeIcon;

            if (PlayerPrefs.HasKey(PlayerPrefsKey)) {
                string json = PlayerPrefs.GetString(PlayerPrefsKey);

                Log($"Loaded existing notifications from PlayerPrefs: {json}");

                _notifications = JsonUtility.FromJson<NotificationsCollection>(json);
            }
            else {
                _notifications = new NotificationsCollection();
            }

            StartCoroutine(PlatformInitRoutine());
        }

        /// <summary>
        /// Schedules a notification with a specified unique ID, title, text, and fire date/time using the default icons.
        /// </summary>
        /// <param name="uniqueId">The unique identifier for the notification.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="text">The text content of the notification.</param>
        /// <param name="fireDateTime">The date and time when the notification should be triggered.</param>
        /// <param name="androidSmallIcon">Optional. The small icon for Android notifications.</param>
        /// <param name="androidLargeIcon">Optional. The large icon for Android notifications.</param>
        public void ScheduleNotification(string uniqueId, string title, string text, DateTime fireDateTime, string androidSmallIcon = "", string androidLargeIcon = "") =>
            ScheduleNotification(uniqueId, title, text, new DateTimeOffset(fireDateTime.ToUniversalTime()).ToUnixTimeSeconds(), androidSmallIcon, androidLargeIcon);

        /// <summary>
        /// Schedules a notification with a specified unique ID, title, text, and fire timestamp.
        /// </summary>
        /// <param name="uniqueId">The unique identifier for the notification.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="text">The text content of the notification.</param>
        /// <param name="fireTimestamp">The timestamp (in seconds) when the notification should be triggered.</param>
        /// <param name="androidSmallIcon">Optional. The small icon for Android notifications.</param>
        /// <param name="androidLargeIcon">Optional. The large icon for Android notifications.</param>
        /// <remarks>
        /// The <paramref name="fireTimestamp"/> must be in seconds and in UTC. The timestamp can also be obtained using the <see cref="GetCurrentTimestamp"/> method.
        /// </remarks>
        public void ScheduleNotification(string uniqueId, string title, string text, long fireTimestamp, string androidSmallIcon = "", string androidLargeIcon = "") {
            Log($"Setting notification \"{uniqueId}\": title=\"{title}\", text=\"{text}\", fireTimestamp=\"{fireTimestamp}\"");

            _notifications.SetNotification(new NotificationsCollection.Notification {
                uniqueId = uniqueId,
                title = title,
                text = text,
                fireTimestamp = fireTimestamp,
                androidSmallIcon = androidSmallIcon,
                androidLargeIcon = androidLargeIcon
            });

            ResetDebounceTimer();
        }

        /// <summary>
        /// Unschedules (removes) a notification specified by its unique ID.
        /// </summary>
        /// <param name="uniqueId">The unique identifier for the notification to be removed.</param>
        public void UnscheduleNotification(string uniqueId) {
            Log($"Removing notification \"{uniqueId}\"");

            if (_notifications.RemoveNotification(uniqueId)) {
                ResetDebounceTimer();
            }
        }

        /// <summary>
        /// Gets the current timestamp in seconds since the Unix epoch.
        /// </summary>
        /// <returns>The current timestamp as a long value.</returns>
        public static long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        #region Private

        // Debouncing

        private void ResetDebounceTimer() {
            if (_debounceTimer < 0) {
                _debounceTimer = _debounceInterval;
                Log("Debounce timer reset");
            }
        }

        private float _debounceTimer = -1;

        public void Update() {
            if (!_initialized || _debounceTimer < 0) {
                return;
            }

            _debounceTimer -= Time.unscaledDeltaTime;

            if (_debounceTimer < 0) {
                RescheduleAndSave();
            }
        }

        private void RescheduleAndSave() {
            _notifications.CleanExpired(GetCurrentTimestamp());

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(_notifications));

            ClearScheduledNotifications();

            foreach (NotificationsCollection.Notification notification in _notifications.list) {
                PlatformScheduleNotification(notification);
            }

            Log("Notifications rescheduled");
        }

        // Other

        private void Log(string message) {
            if (!_loggingEnabled) {
                return;
            }

            Debug.Log($"[SUN] {message}");
        }

        #endregion

        #region Private platform-specific

        private IEnumerator PlatformInitRoutine() {
#if UNITY_ANDROID
            AndroidNotifications.RequestAuthorization();
            AndroidNotifications.RegisterNotificationChannel();
            yield return AndroidNotifications.RequestNotificationPermission();
#elif UNITY_IOS
            yield return iOSNotifications.RequestAuthorization();
#else
            yield return null;
#endif
            Log("Initialized");
            _initialized = true;
        }

        private static void ClearScheduledNotifications() {
#if UNITY_ANDROID
            AndroidNotifications.ClearScheduledNotifications();
#elif UNITY_IOS
            iOSNotifications.ClearScheduledNotifications();
#endif
        }

        private void PlatformScheduleNotification(NotificationsCollection.Notification notification) {
#if UNITY_ANDROID
            string smallIcon = notification.androidSmallIcon;
            if (string.IsNullOrEmpty(smallIcon)) {
                smallIcon = _defaultAndroidSmallIcon;
            }

            string largeIcon = notification.androidLargeIcon;
            if (string.IsNullOrEmpty(largeIcon)) {
                largeIcon = _defaultAndroidLargeIcon;
            }

            AndroidNotifications.ScheduleNotification(notification.title,
                notification.text,
                DateTimeOffset.FromUnixTimeSeconds(notification.fireTimestamp).LocalDateTime,
                smallIcon,
                largeIcon
            );
#elif UNITY_IOS
            iOSNotifications.ScheduleNotification(
                notification.title,
                notification.text,
                TimeSpan.FromSeconds(notification.fireTimestamp - GetCurrentTimestamp())
            );
#endif
        }

        /// <summary>
        /// Clears all delivered notifications from the system.
        /// </summary>
        public void ClearDeliveredNotifications() {
#if UNITY_ANDROID
            AndroidNotifications.ClearDeliveredNotifications();
#elif UNITY_IOS
            iOSNotifications.ClearDeliveredNotifications();
#endif
        }

        #endregion
    }
}