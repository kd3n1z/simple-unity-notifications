# Simple Unity Notifications

SUN is a lightweight, easy-to-use [Unity Mobile Notifications](https://github.com/Unity-Technologies/com.unity.mobile.notifications) wrapper for managing mobile notifications in Unity projects.

## Installation

-   Open Package Manager
-   Add package from git URL:
    `https://github.com/kd3n1z/simple-unity-notifications.git`

## Usage

### Initialization

To use NotificationsManager, you need to call the `Initialize` method to configure and prepare the manager for handling notifications. This method takes several optional parameters that allow you to customize its behavior:

-   `defaultAndroidSmallIcon` (string)
    -   Specifies the default small icon name used in Android notifications. This should match an icon in `Project Settings > Mobile Notifications > Android > Notification Icons`
    -   **Default Value**: `""`
-   `defaultAndroidLargeIcon` (string)
    -   Specifies the default large icon name used in Android notifications. This should match an icon in `Project Settings > Mobile Notifications > Android > Notification Icons`
    -   **Default Value**: `""`
-   `debounceInterval` (float)
    -   The time interval (in seconds) used to control how frequently notifications are rescheduled and saved. This helps reduce the overhead of scheduling notifications in quick succession.
    -   **Default Value**: `1`
-   `loggingEnabled` (bool)
    -   A flag that enables or disables debug logging within the `NotificationsManager`. When set to `true`, messages are logged to the Unity console for better visibility during development and debugging.
    -   **Default Value**: `false`

Example:

```csharp
// Initialize the notifications manager
notificationsManager.Initialize(
    defaultAndroidSmallIcon: "icon_small",
    defaultAndroidLargeIcon: "icon_large",
    debounceInterval: 2.0f
    loggingEnabled: true
);
```

### Notifications

#### Scheduling

To schedule a notification, use the `SetNotification` method provided by the `NotificationsManager`. This method allows you to create notifications by specifying a unique identifier, title, content text, and the time at which the notification should fire. The notification time can be set using either a `DateTime` object or a **UTC Unix timestamp (in seconds)**. Additionally, you can specify Android icons.

-   `uniqueId` (string): A unique identifier for the notification.
-   `title` (string): The title displayed at the top of the notification.
-   `text` (string): The main content/body of the notification.
-   `fireDateTime` (DateTime): The date and time at which the notification should trigger.
-   `fireTimestamp` (long): A **UTC Unix timestamp (in seconds)** specifying when the notification should trigger.
    > Use `NotificationsManager.GetCurrentTimestamp()` to obtain the current time in UTC for accurate scheduling.
-   `androidSmallIcon` (string, optional): Specifies the small icon for the notification, overriding the default set in initialization.
-   `androidLargeIcon` (string, optional): Specifies the large icon for the notification, overriding the default set in initialization.

> [!IMPORTANT]
> The `uniqueId` parameter is used to override existing notifications. If a notification with the same `uniqueId` already exists, it will be updated with the new details. Ensure that you use a unique ID for each notification unless you intend to replace an existing one.

Example using `DateTime`:

```csharp
// Schedule a notification for when lives are fully restored in 1 hour
notificationsManager.SetNotification(
    "lives_restored",
    "Lives Restored!",
    "Your lives have been fully restored. Jump back into the game!",
    DateTime.Now.AddHours(1),
    androidSmallIcon: "custom_icon_small", // Optional override
    androidLargeIcon: "custom_icon_large"  // Optional override
);
```

Example using a **Unix timestamp**:

```csharp
// Schedule a notification for when lives are fully restored in 1 hour (3600 seconds)
notificationsManager.SetNotification(
    "lives_restored",
    "Lives Restored!",
    "Your lives have been fully restored. Jump back into the game!",
    NotificationsManager.GetCurrentTimestamp() + 3600,
    androidSmallIcon: "custom_icon_small", // Optional override
    androidLargeIcon: "custom_icon_large"  // Optional override
);
```

#### Unscheduling

To remove a notification, use the `RemoveNotification` method.

-   `uniqueId` (string): A unique identifier for the notification.

> [!NOTE]
> You can safely remove a notification even if it hasn't been set; this will not cause any errors.

Example:

```csharp
// Remove a previously scheduled notification
notificationsManager.RemoveNotification("lives_restored");
```

That's it. The `NotificationsManager` automatically persists and reschedules notifications as needed, while the `debounceInterval` prevents frequent updates.

## Recommended Project Settings

Navigate to `Project Settings > Mobile Notifications` and configure the following for optimal functionality:

| Platform | Setting                                    | Value                  |
| -------- | ------------------------------------------ | ---------------------- |
| Android  | Reschedule on Device Restart               | `true`                 |
|          | Schedule at Exact Time                     | `Exact when available` |
| iOS      | Request Authorization on App Launch        | `true`                 |
|          | Default Notification Authorization Options | `Badge, Sound, Alert`  |
|          | Enable Time Sensitive Notifications        | `true`                 |

## Notification References

<details>
  <summary><strong>iOS (iPhone 16 Pro)</strong></summary>
  
  <img alt="iOS Notification (iPhone 16 Pro)" src="Images~/iOS Notification.png" width="585"/>
</details>

<details>
  <summary><strong>Android (Pixel Fold)</strong></summary>
  
  <img alt="Android Notification (Pixel Fold)" src="Images~/Android Pixel Notification.png" width="523"/>
  
  <ul>
    <li><strong>S</strong> is depicted on the small icon and <strong>L</strong> on the large one.</li>
    <li>The accent color of the notification is <code>#ffc080</code> (you can see it on the small icon).</li>
  </ul>
</details>

## License

This project is licensed under the MIT license. See the [LICENSE.md](LICENSE.md) file for details.
