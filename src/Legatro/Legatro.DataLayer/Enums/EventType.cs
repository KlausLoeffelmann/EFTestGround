namespace Legatro.DataLayer.Enums;

/// <summary>
/// Defines the type of event represented by a TimeItem.
/// </summary>
public enum EventType : short
{
    /// <summary>
    /// Default event type
    /// </summary>
    Default = 0,

    /// <summary>
    /// Time tracking event
    /// </summary>
    Time = 1,

    /// <summary>
    /// Generic task event
    /// </summary>
    Task = 10,

    /// <summary>
    /// Task planning event
    /// </summary>
    TaskPlanning = 11,

    /// <summary>
    /// Task execution event
    /// </summary>
    TaskExecution = 12,

    /// <summary>
    /// Task completion event
    /// </summary>
    TaskCompletion = 13,

    /// <summary>
    /// Task review event
    /// </summary>
    TaskReview = 14,

    /// <summary>
    /// Task approval event
    /// </summary>
    TaskApproval = 15,

    /// <summary>
    /// Task rejection event
    /// </summary>
    TaskRejection = 16,

    /// <summary>
    /// Task cancellation event
    /// </summary>
    TaskCancellation = 17,

    /// <summary>
    /// Task escalation event
    /// </summary>
    TaskEscalation = 18,

    /// <summary>
    /// Task reassignment event
    /// </summary>
    TaskReassignment = 19,

    /// <summary>
    /// Comment event
    /// </summary>
    Comment = 100,

    /// <summary>
    /// Location change event
    /// </summary>
    Location = 200,

    /// <summary>
    /// Amount capture event
    /// </summary>
    AmountCapture = 300,

    /// <summary>
    /// Action log event
    /// </summary>
    ActionLog = 400,

    /// <summary>
    /// Notification area start marker
    /// </summary>
    NotificationAreaStart = 11000,

    /// <summary>
    /// Information notification
    /// </summary>
    NotificationInfo = 11001,

    /// <summary>
    /// Warning notification
    /// </summary>
    NotificationWarning = 11002,

    /// <summary>
    /// Error notification
    /// </summary>
    NotificationError = 11003,

    /// <summary>
    /// Success notification
    /// </summary>
    NotificationSuccess = 11004,

    /// <summary>
    /// Approval request notification
    /// </summary>
    NotificationApprovalRequest = 11005,

    /// <summary>
    /// Task assignment notification
    /// </summary>
    NotificationTaskAssignment = 11006,

    /// <summary>
    /// Task deadline notification
    /// </summary>
    NotificationTaskDeadline = 11007,

    /// <summary>
    /// Reminder notification
    /// </summary>
    NotificationReminder = 11008,

    /// <summary>
    /// System notification
    /// </summary>
    NotificationSystem = 11009,

    /// <summary>
    /// Notification area end marker
    /// </summary>
    NotificationAreaEnd = 11999
}