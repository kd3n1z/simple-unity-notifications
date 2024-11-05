using System;
using System.Collections.Generic;
using System.Linq;

namespace Sun {
    [Serializable]
    internal class NotificationsCollection {
        public List<Notification> list = new List<Notification>();

        public Notification this[string notificationUniqueId] {
            get {
                foreach (Notification notification in list) {
                    if (notification.uniqueId == notificationUniqueId) {
                        return notification;
                    }
                }

                throw new KeyNotFoundException();
            }

            set {
                for (int i = 0; i < list.Count; i++) {
                    if (list[i].uniqueId == notificationUniqueId) {
                        list[i] = value;
                        return;
                    }
                }

                list.Add(value);
            }
        }

        public void CleanExpired(long currentTimestamp) {
            list = list.Where(e => e.fireTimestamp > currentTimestamp).ToList();
        }

        [Serializable]
        internal class Notification {
            public string uniqueId;
            public string title;
            public string text;
            public long fireTimestamp;
            public string androidSmallIcon;
            public string androidLargeIcon;
        }
    }
}