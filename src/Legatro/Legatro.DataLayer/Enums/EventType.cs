namespace Legatro.DataLayer.Enums;

/// <summary>
/// Provides the list of possible Event Types which can be reflected by a TimeItem.
/// </summary>
public enum EventType
{
    Default = 0,
    Time = 1,

    Task = 10,
    TfsTask = 11,
    OutlookTask = 12,
    TodoTask = 13,
    GitHubIssue = 14,
    OtherTask = 19,

    Comment = 100,
    Location = 200,
    AmountCapture = 300,
    ActionLog = 400,

    /// <summary>
    /// Defines the start of the Notification Area in this Enum
    /// to query between EventTypes of type Notification.
    /// </summary>
    NotificationAreaStart = 11000,

    // Team and Team users
    UserRequestSent = 11100,
    UserRequestReceived = 11101,
    UserRequestAcceptanceSent = 11102,
    UserRequestAcceptanceReceived = 11103,
    UserRequestDenialSent = 11104,
    UserRequestDenialReceived = 11105,
    UserRemovalNotificationSent = 11106,
    UserRemovalNotificationReceived = 11107,

    // Projects
    ProjectInvitationSent = 11200,
    ProjectInvitationReceived = 11201,
    ProjectRemovalNotificationSent = 11202,
    ProjectRemovalNotificationReceived = 11203,

    // Tasks
    TaskRequestSent = 11300,
    TaskRequestReceived = 11301,
    TaskRequestAcceptanceSent = 11302,
    TaskRequestAcceptanceReceived = 11303,
    TaskRequestDenialSent = 11304,
    TaskRequestDenialReceived = 11305,
    TaskRemovedNotificationSent = 11306,
    TaskRemovedNotificationReceived = 11307,
    TaskFinishedNotificationSent = 11308,
    TaskFinishedNotificationReceived = 11309,
    TaskReturnedNotificationSent = 11310,
    TaskReturnedNotificationReceived = 11311,

    // Posting
    TeamBroadcastPublicPosting = 11801,
    TeamBroadcastInternalPosting = 11802,
    TeamMemberPostingSent = 11820,
    TeamMemberPostingReceived = 11821,

    // System
    SystemBroadcastUpdate = 11901,
    SystemBroadcastMessage = 11902,

    /// <summary>
    /// Defines the end of the Notification Area in this Enum
    /// to query between EventTypes of type Notification.
    /// </summary>
    NotificationAreaEnd = 11999
}
