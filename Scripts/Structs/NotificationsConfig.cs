namespace Sun {
    public struct NotificationsConfig {
        public float DebounceInterval;
        public float MaxDebounceInterval;
        public bool AutoClearDelivered;

        public static readonly NotificationsConfig Default = new NotificationsConfig() {
            DebounceInterval = 1,
            MaxDebounceInterval = 5,
            AutoClearDelivered = true,
        };
    }
}