using System;
using System.Collections;
using UnityEngine;

namespace Sun {
    /// <summary>
    /// Manages the scheduling, persistence, and authorization of notifications. This manager handles the lifecycle of notifications,
    /// including rescheduling, clearing delivered notifications, and integrating with platform-specific notification systems.
    /// </summary>
    public class NotificationsManager : MonoBehaviour {
        private const string PlayerPrefsKey = nameof(Sun) + "." + nameof(NotificationsManager) + ".Config";

#if UNITY_EDITOR || UNITY_DEVELOPMENT
        [SerializeField] private bool debug;
#endif

        private bool _initialized;
        private SerializableDictionary<string, SavedNotification> _notifications;

        #region Configuration & Defaults

        private NotificationsConfig _config;
        private CommonOptions _defaultCommonOptions;
        private AndroidOptions _defaultAndroidOptions;

        #endregion

        #region Unity Event Methods

        private bool _rescheduleRequested;
        private float _throttleTimer;
        private float _debounceTimer;

        private void Update() {
            if (!_initialized || !_rescheduleRequested) {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;

            _debounceTimer -= deltaTime;
            _throttleTimer -= deltaTime;

            if (_debounceTimer < 0 || _throttleTimer < 0) {
                Reschedule();
            }
        }

        private void OnApplicationFocus(bool hasFocus) {
            if (!_initialized || !hasFocus || !_config.AutoClearDelivered) {
                return;
            }

            ClearDelivered();
        }

        #endregion

        #region Private

        private void RequestReschedule() {
            if (!_rescheduleRequested) {
                _throttleTimer = _config.ThrottleTimeout;
            }

            _rescheduleRequested = true;
            _debounceTimer = _config.DebounceTimeout;
        }

        private void Reschedule() {
            _rescheduleRequested = false;

            long currentTimestamp = DateTime.Now.ToUnixTimestamp();

            for (int i = 0; i < _notifications.values.Count;) {
                if (currentTimestamp > _notifications.values[i].fireTimestamp) {
                    _notifications.RemoveAt(i);
                    continue;
                }

                i++;
            }

            string json = JsonUtility.ToJson(_notifications);
            PlayerPrefs.SetString(PlayerPrefsKey, json);

#if UNITY_EDITOR || UNITY_DEVELOPMENT
            Log($"Rescheduling notifications: {json}");
#endif

#if UNITY_ANDROID
            AndroidNotifications.ClearScheduled();
#elif UNITY_IOS
            iOSNotifications.ClearScheduled();
#endif

            foreach (SavedNotification n in _notifications.values) {
#if UNITY_ANDROID
                AndroidNotifications.Schedule(
                    DateTimeOffset.FromUnixTimeSeconds(n.fireTimestamp).DateTime,
                    n.title,
                    n.text,
                    n.androidSmallIconName,
                    n.androidLargeIconName,
                    new Color(n.androidAccentColorR, n.androidAccentColorG, n.androidAccentColorB, n.androidAccentColorA)
                );
#elif UNITY_IOS
                iOSNotifications.Schedule(
                    TimeSpan.FromSeconds(n.fireTimestamp - currentTimestamp),
                    n.title,
                    n.text
                );
#endif
            }
        }

        private IEnumerator RequestAuthorizationRoutine() {
#if UNITY_ANDROID
            AndroidNotifications.RequestAuthorization();
            AndroidNotifications.RegisterNotificationChannel();
            yield return AndroidNotifications.RequestNotificationPermission();
#elif UNITY_IOS
            yield return iOSNotifications.RequestAuthorization();
#else
            yield return null;
#endif

            _initialized = true;

#if UNITY_EDITOR || UNITY_DEVELOPMENT
            Log("Initialized");
#endif

            if (_config.AutoClearDelivered) {
                ClearDelivered();
            }
        }

#if UNITY_EDITOR || UNITY_DEVELOPMENT
        private void Log(string message) {
            if (!debug) {
                return;
            }

            Debug.Log($"[{nameof(Sun)}] {message}");
        }
#endif

        #endregion

        #region Public

        /// <summary>
        /// Initializes the notification manager with the specified configuration and default options.
        /// Loads existing notifications from persistent storage if available, then requests notification authorization.
        /// </summary>
        /// <param name="config">The configuration settings for notifications.</param>
        /// <param name="defaultCommonOptions">Optional default common options for notifications.</param>
        /// <param name="defaultAndroidOptions">Optional default Android-specific options for notifications.</param>
        public void Initialize(NotificationsConfig config, CommonOptions? defaultCommonOptions = null, AndroidOptions? defaultAndroidOptions = null) {
            if (_initialized) {
                throw new Exception($"{nameof(NotificationsManager)} already initialized");
            }

            if (PlayerPrefs.HasKey(PlayerPrefsKey)) {
                string json = PlayerPrefs.GetString(PlayerPrefsKey);

#if UNITY_EDITOR || UNITY_DEVELOPMENT
                Log($"Loaded existing notifications from PlayerPrefs: {json}");
#endif

                _notifications = JsonUtility.FromJson<SerializableDictionary<string, SavedNotification>>(json);
            }
            else {
                _notifications = new SerializableDictionary<string, SavedNotification>();
            }

            _config = config;
            _defaultCommonOptions = defaultCommonOptions ?? CommonOptions.Default;
            _defaultAndroidOptions = defaultAndroidOptions ?? AndroidOptions.Default;

            StartCoroutine(RequestAuthorizationRoutine());
        }

        /// <summary>
        /// Schedules a new notification with the specified ID and fire time, overwriting any existing notification with the same ID.
        /// </summary>
        /// <param name="id">Unique identifier for the notification, used to overwrite existing notifications if present.</param>
        /// <param name="fireTimestamp">The time at which the notification should be fired.</param>
        /// <param name="commonOverrides">Optional overrides for common notification options.</param>
        /// <param name="androidOverrides">Optional overrides for Android-specific notification options.</param>
        public void Schedule(string id, DateTime fireTimestamp, CommonOptions? commonOverrides = null, AndroidOptions? androidOverrides = null) {
            _notifications[id] = new SavedNotification(
                fireTimestamp,
                _defaultCommonOptions.Override(commonOverrides),
                _defaultAndroidOptions.Override(androidOverrides)
            );

            RequestReschedule();
        }

        /// <summary>
        /// Removes a scheduled notification by its identifier.
        /// </summary>
        /// <param name="id">Unique identifier for the notification to remove.</param>
        public void Unschedule(string id) {
            _notifications.Remove(id);
            RequestReschedule();
        }

        /// <summary>
        /// Clears delivered notifications.
        /// </summary>
        public void ClearDelivered() {
#if UNITY_EDITOR || UNITY_DEVELOPMENT
            Log("Clearing delivered notifications");
#endif

#if UNITY_ANDROID
            AndroidNotifications.ClearDelivered();
#elif UNITY_IOS
            iOSNotifications.ClearDelivered();
#endif
        }

        #endregion
    }
}