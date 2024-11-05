using System;
using System.Collections;
using UnityEngine;

namespace Sun {
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

        public void SetNotification(string uniqueId, string title, string text, DateTime fireDateTime, string androidSmallIcon = "", string androidLargeIcon = "") =>
            SetNotification(uniqueId, title, text, new DateTimeOffset(fireDateTime.ToUniversalTime()).ToUnixTimeSeconds(), androidSmallIcon, androidLargeIcon);

        public void SetNotification(string uniqueId, string title, string text, long fireTimestamp, string androidSmallIcon = "", string androidLargeIcon = "") {
            Log($"Setting notification \"{uniqueId}\": title=\"{title}\", text=\"{text}\", fireTimestamp=\"{fireTimestamp}\"");

            if (_debounceTimer < 0) {
                _debounceTimer = _debounceInterval;
                Log("Debounce timer reset");
            }

            _notifications[uniqueId] = new NotificationsCollection.Notification {
                uniqueId = uniqueId,
                title = title,
                text = text,
                fireTimestamp = fireTimestamp,
                androidSmallIcon = androidSmallIcon,
                androidLargeIcon = androidLargeIcon
            };
        }

        public static long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        #region Private

        // Debouncing

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
                ScheduleNotification(notification);
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

        private void ScheduleNotification(NotificationsCollection.Notification notification) {
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

        #endregion
    }
}