namespace Sun {
    public struct NotificationsConfig {
        public float DebounceTimeout;
        public float ThrottleTimeout;
        public bool AutoClearDelivered;

        public static readonly NotificationsConfig Default = new NotificationsConfig() {
            DebounceTimeout = 1,
            ThrottleTimeout = 5,
            AutoClearDelivered = true,
        };
    }
}