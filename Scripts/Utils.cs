using System;

namespace Sun {
    public static class Utils {
        public static long ToUnixTimestamp(this DateTime dateTime) {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }
    }
}