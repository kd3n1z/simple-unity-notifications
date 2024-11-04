# Simple Unity Notifications

SUN is a lightweight, easy-to-use [Unity Mobile Notifications](https://github.com/Unity-Technologies/com.unity.mobile.notifications) wrapper for managing mobile notifications in Unity projects.

## Installation

-   Open Package Manager
-   Add package from git URL:
    <code>https://github.com/kd3n1z/simple-unity-notifications.git</code>

## Usage

### Initialization

To use NotificationsManager, you need to call the <code>Initialize</code> method to configure and prepare the manager for handling notifications. This method takes several optional parameters that allow you to customize its behavior:

-   <code>androidSmallIcon</code> (string)
    -   Specifies the name of the small icon used in Android notifications. This should correspond to the name of an icon in the <code>Project Settings > Mobile Notifications > Android > Notification Icons</code>
    -   **Default Value**: <code>""</code>
-   <code>androidLargeIcon</code> (string)
    -   Specifies the name of the large icon used in Android notifications. This should correspond to the name of an icon in the <code>Project Settings > Mobile Notifications > Android > Notification Icons</code>
    -   **Default Value**: <code>""</code>
-   <code>debounceInterval</code> (float)
    -   The time interval (in seconds) used to control how frequently notifications are rescheduled and saved. This helps reduce the overhead of scheduling notifications in quick succession.
    -   **Default Value**: <code>1</code>
-   <code>loggingEnabled</code> (bool)
    -   A flag that enables or disables debug logging within the <code>NotificationsManager</code>. When set to <code>true</code>, messages are logged to the Unity console for better visibility during development and debugging.
    -   **Default Value**: <code>false</code>

<br/>

```csharp
notificationsManager.Initialize(
    androidSmallIcon: "icon_small",  // Uses a specific small icon for Android
    androidLargeIcon: "icon_large",  // Uses a specific large icon for Android
    debounceInterval: 2.0f,          // Limits rescheduling operations to once every 2 seconds
    loggingEnabled: true             // Enables logging for debugging
);
```

### Notifications Scheduling

To schedule a notification, use the <code>SetNotification</code> method provided by the <code>NotificationsManager</code>. This method allows you to create notifications by specifying a unique identifier, title, content text, and the time at which the notification should fire. The notification time can be set using either a <code>DateTime</code> object or a **UTC Unix timestamp (in seconds)**.

-   <code>uniqueId</code> (string): A unique identifier for the notification.
-   <code>title</code> (string): The title displayed at the top of the notification.
-   <code>text</code> (string): The main content/body of the notification.
-   <code>fireDateTime</code> (DateTime): The date and time at which the notification should trigger.
-   <code>fireTimestamp</code> (long): A **UTC Unix timestamp (in seconds)** specifying when the notification should trigger.
    > Use <code>NotificationsManager.GetCurrentTimestamp()</code> to obtain the current time in UTC for accurate scheduling.

> [!IMPORTANT]
> The <code>uniqueId</code> parameter is used to override existing notifications. If a notification with the same <code>uniqueId</code> already exists, it will be updated with the new details. Ensure that you use a unique ID for each notification unless you intend to replace an existing one.

<br/>

Example using <code>DateTime</code>:

```csharp
// Schedule a notification for when lives are fully restored in 1 hour
notificationsManager.SetNotification(
    "lives_restored",
    "Lives Restored!",
    "Your lives have been fully restored. Jump back into the game!",
    DateTime.Now.AddHours(1)
);
```

Example using a **Unix timestamp**:

```csharp
// Schedule a notification for when lives are fully restored in 1 hour (3600 seconds)
notificationsManager.SetNotification(
    "lives_restored",
    "Lives Restored!",
    "Your lives have been fully restored. Jump back into the game!",
    NotificationsManager.GetCurrentTimestamp() + 3600
);
```

<br/>

That's it. The <code>NotificationsManager</code> automatically persists and reschedules notifications as needed, while the <code>debounceInterval</code> prevents frequent updates.

## License

This project is licensed under the MIT license. See the [LICENSE.md](LICENSE.md) file for details.
