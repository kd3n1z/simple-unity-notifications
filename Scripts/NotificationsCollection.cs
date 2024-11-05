using System;
using System.Collections.Generic;
using System.Linq;

namespace Sun {
    [Serializable]
    internal class NotificationsCollection {
        public List<Notification> list = new List<Notification>();

        public Notification GetNotification(string notificationUniqueId) {
            foreach (Notification notification in list) {
                if (notification.uniqueId == notificationUniqueId) {
                    return notification;
                }
            }

            throw new KeyNotFoundException();
        }

        public void SetNotification(Notification notification) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].uniqueId == notification.uniqueId) {
                    list[i] = notification;

                    return;
                }
            }

            list.Add(notification);
        }

        public bool RemoveNotification(string uniqueId) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].uniqueId == uniqueId) {
                    list.RemoveAt(i);

                    return true;
                }
            }

            return false;
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