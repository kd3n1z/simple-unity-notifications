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

        #region Defaults

        private Config _config = Config.Default;
        private AndroidOptions _defaultAndroidOptions;

        #endregion

        /// <summary>
        /// Initializes the <see cref="NotificationsManager"/> with optional parameters for default icons, debounce interval, and logging.
        /// </summary>
        /// <exception cref="Exception">Thrown if the manager is already initialized.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the debounce interval is negative.</exception>
        public void Initialize(Config? config = null, AndroidOptions? defaultAndroidOptions = null) {
            if (_initialized) {
                throw new Exception($"{nameof(NotificationsManager)} is already initialized");
            }

            if (config.HasValue) {
                Config configValue = config.Value;

                if (configValue.DebounceInterval.HasValue) {
                    _config.DebounceInterval = configValue.DebounceInterval.Value;
                }

                if (configValue.EnableLogging.HasValue) {
                    _config.EnableLogging = configValue.EnableLogging.Value;
                }
            }

            _defaultAndroidOptions = AndroidOptions.Override(AndroidOptions.Default, defaultAndroidOptions);

            if (_config.DebounceInterval < 0) {
                throw new ArgumentOutOfRangeException();
            }

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
        public void ScheduleNotification(string uniqueId, string title, string text, DateTime fireDateTime, AndroidOptions? androidOptions = null) =>
            ScheduleNotification(uniqueId, title, text, new DateTimeOffset(fireDateTime.ToUniversalTime()).ToUnixTimeSeconds(), androidOptions);

        /// <summary>
        /// Schedules a notification with a specified unique ID, title, text, and fire timestamp.
        /// </summary>
        /// <param name="uniqueId">The unique identifier for the notification.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="text">The text content of the notification.</param>
        /// <param name="fireTimestamp">The timestamp (in seconds) when the notification should be triggered.</param>
        /// <remarks>
        /// The <paramref name="fireTimestamp"/> must be in seconds and in UTC. The timestamp can also be obtained using the <see cref="GetCurrentTimestamp"/> method.
        /// </remarks>
        public void ScheduleNotification(string uniqueId, string title, string text, long fireTimestamp, AndroidOptions? androidOptions = null) {
            Log($"Setting notification \"{uniqueId}\": title=\"{title}\", text=\"{text}\", fireTimestamp=\"{fireTimestamp}\"");

#if UNITY_ANDROID
            AndroidOptions options = AndroidOptions.Override(_defaultAndroidOptions, androidOptions);

            // ReSharper disable once PossibleInvalidOperationException
            Color accentColor = options.AccentColor.Value;
#endif

            _notifications.SetNotification(new NotificationsCollection.Notification {
                uniqueId = uniqueId,
                title = title,
                text = text,
                fireTimestamp = fireTimestamp,
#if UNITY_ANDROID
                androidSmallIcon = options.SmallIconName,
                androidLargeIcon = options.LargeIconName,
                androidAccentColorR = accentColor.r,
                androidAccentColorG = accentColor.g,
                androidAccentColorB = accentColor.b,
                androidAccentColorA = accentColor.a
#endif
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

        /// <summary>
        /// Clears all delivered notifications from the system.
        /// </summary>
        public void ClearDeliveredNotifications() {
            Log("Clearing delivered notifications");

#if UNITY_ANDROID
            AndroidNotifications.ClearDeliveredNotifications();
#elif UNITY_IOS
            iOSNotifications.ClearDeliveredNotifications();
#endif
        }

        #region Private

        // Debouncing

        private void ResetDebounceTimer() {
            if (_debounceTimer < 0) {
                // ReSharper disable once PossibleInvalidOperationException
                _debounceTimer = _config.DebounceInterval.Value;
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
            // ReSharper disable once PossibleInvalidOperationException
            if (!_config.EnableLogging.Value) {
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

        private static void PlatformScheduleNotification(NotificationsCollection.Notification notification) {
#if UNITY_ANDROID
            AndroidNotifications.ScheduleNotification(notification.title,
                notification.text,
                DateTimeOffset.FromUnixTimeSeconds(notification.fireTimestamp).LocalDateTime,
                notification.androidSmallIcon,
                notification.androidLargeIcon,
                new Color(notification.androidAccentColorR, notification.androidAccentColorG, notification.androidAccentColorB, notification.androidAccentColorA)
            );
#elif UNITY_IOS
            iOSNotifications.ScheduleNotification(
                notification.title,
                notification.text,
                TimeSpan.FromSeconds(notification.fireTimestamp - GetCurrentTimestamp())
            );
#endif
        }

        #endregion

        public struct Config {
            public float? DebounceInterval;
            public bool? EnableLogging;

            public static Config Default => new Config {
                DebounceInterval = 1f,
                EnableLogging = false
            };
        }
    }
}