namespace Sun {
    /// <summary>
    /// Configuration settings for managing notification behavior, such as debounce and throttle timeouts.
    /// </summary>
    public struct NotificationsConfig {
        /// <summary>
        /// The minimum time interval (in seconds) between notification reschedules.
        /// </summary>
        public float DebounceTimeout;

        /// <summary>
        /// The maximum time interval (in seconds) before notifications are forcefully rescheduled.
        /// </summary>
        public float ThrottleTimeout;

        /// <summary>
        /// Determines whether delivered notifications should be automatically cleared when the application gains focus.
        /// </summary>
        public bool AutoClearDelivered;

        /// <summary>
        /// Default configuration values for notifications.
        /// </summary>
        public static readonly NotificationsConfig Default = new NotificationsConfig() {
            DebounceTimeout = 1,
            ThrottleTimeout = 5,
            AutoClearDelivered = true,
        };
    }
}