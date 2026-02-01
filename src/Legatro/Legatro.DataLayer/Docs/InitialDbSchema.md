# Entity: TimeItem

| Property                    | Type            | MaxLen | ColType  | Index                                        | Track | Notes                                         |
|-----------------------------|-----------------|--------|----------|----------------------------------------------|-------|-----------------------------------------------|
| IdTimeItem                  | guid            |        | guid     | PrimaryIndex                                 |       |                                               |
| IdUser                      | guid            |        | guid     | NavigationProperty                           |       |                                               |
| IdProject                   | guid?           |        | guid?    | NavigationProperty                           |       |                                               |
| IdTask                      | guid?           |        | guid?    | NavigationProperty                           |       |                                               |
| IdTimeItemCategory          | guid?           |        | guid?    | NavigationProperty                           |       | TimeItem/Task category                        |
| IdTimeItemType              | guid?           |        | guid?    | NavigationProperty                           |       | Booking type (CheckIn, CheckOut, Break, etc.) |
| EventInfo                   | string          | 100    | nvarchar |                                              |       | Debug message for event                       |
| EventType                   | EventType       |        |          | Yes                                          |       | Enum                                          |
| IdHistoryParent             | guid            |        | guid     |                                              |       | We never edit.                                |
|                             |                 |        |          |                                              |       | We archive, set the valid flags, and create a |
|                             |                 |        |          |                                              |       | new item.									   |
| ShortTitel                  | string?         | 2000   | nvarchar |                                              |       | Short description                             |
| Description                 | string?         |        | text     |                                              |       | Full description                              |
| IdNextItem                  | guid?           |        | guid?    |                                              |       | Link to next item, self-reference                             |
| DurationToNext              | TimeSpan?       |        |          |                                              | Yes   |                                               |
| DurationTicksToNext         | long?           |        |          |                                              | Yes   | For SQL aggregation                           |
| IdPreviousItem              | guid?           |        | guid?    |                                              | Yes   | Link to previous item, self-reference                         |
| DurationToPrevious          | TimeSpan?       |        |          |                                              | Yes   |                                               |
| DurationTicksToPrevious     | long?           |        |          |                                              | Yes   | For SQL aggregation                           |
| ItemCompletedRequestDate    | DateTimeOffset? |        |          |                                              |       |                                               |
| IdUserItemFrom              | guid?           |        | guid?    | NavigationProperty                           |       | Requester user ID                             |
| DateItemAcceptedOrRejected  | DateTimeOffset? |        |          |                                              |       |                                               |
| DateItemFinished            | DateTimeOffset? |        |          |                                              |       |                                               |
| IdParentTimeItem            | guid?           |        | guid?    | NavigationProperty                           |       | Parent task reference, Self-Reference         |
| ExternalReferenceId         | string?         | 128    | nvarchar | Unique Index                                 |       | Microsoft To-Do, AzDO, Git, SAP, etc.         |
| LastNotificationSentDate    | DateTimeOffset? |        |          |                                              |       |                                               |
| NotificationAcknowledgedDate| DateTimeOffset? |        |          |                                              |       |                                               |
| IsTaggedForDueNotification  | bool            |        |          |                                              |       |                                               |
| IsNewQuickItem              | bool            |        |          |                                              |       | Quick task flag, may become obsolete          |
| IsCompleted                 | bool            |        |          | Index                                        |       |                                               |
| IsDeleted                   | bool            |        |          | Index                                        |       |                                               |
| IsStartAction               | bool            |        |          |                                              |       | Stops backward chain traversal                |
| IsEndAction                 | bool            |        |          |                                              |       | Stops forward chain traversal                 |
| IsAssignmentRejected        | bool            |        |          |                                              |       |                                               |
| EventTime                   | DateTimeOffset? |        |          | Index                                        | Yes   | Event time or task due time                   |
| BookingDateGMT              | DateTime?       |        |          | Index                                        |       | Absolute day date, GMT                        |
| Value                       | decimal?        |        |          |                                              |       | Entity value or % done                        |
| Priority                    | int             |        |          |                                              |       | 1=highest, 1000=standard/low                  |
| SortOrder                   | double          |        |          |                                              |       | Sort order for same priority/date             |
| MetaInfo                    | string?         |        | text     |                                              |       | Additional JSON data                          |
| DeviceInfo                  | string?         | 255    | nvarchar |                                              |       |                                               |
| LocationInfo                | string?         | 255    | nvarchar |                                              |       |                                               |
| Location                    | DbGeography?    |        |          |                                              |       |                                               |
| DateLastEdited              | DateTime        |        | datetime |                    |       |                                               |
| DateValidTo                 | DateTime?       |        | datetime |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime |                    |       |                                               |
| SyncGuid                    | guid            |        | guid     | Unique                                       |       | Sync check with last changed date             |

## Enums

| Name             | Values                                                              |
|------------------|---------------------------------------------------------------------|
| EventType        | see below                                                           |

### EventType Enum Values

```csharp
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
```

---

# Entity: User

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdUser                      | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdUserForShadow             | guid?           |        | guid?             | NavigationProperty |       |                                               |
| IdContact                   | guid            |        | guid              | NavigationProperty |       |                                               |
| ExternalReferenceId         | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| IsShadowEmployee            | bool            |        | bit               |                    |       |                                               |
| AutoBookForShadowImplicitly | bool            |        | bit               |                    |       |                                               |
| PersonnelNumber             | string?         | 50     | nvarchar          |                    |       |                                               |
| Matchcode                   | string?         | 50     | nvarchar          |                    |       |                                               |
| LastName                    | string          | 100    | nvarchar          |                    |       |                                               |
| FirstName                   | string          | 100    | nvarchar          |                    |       |                                               |
| MiddleName                  | string?         | 100    | nvarchar          |                    |       |                                               |
| Username                    | string          | 100    | nvarchar          |                    |       |                                               |
| Password                    | byte[]?         | 128    | varbinary         |                    |       |                                               |
| ClearanceLevel              | long            |        | bigint            |                    |       |                                               |
| DateLastTriggered           | DateTime?       |        | datetime          |                    |       |                                               |
| IsAdmin                     | bool            |        | bit               |                    |       |                                               |
| IsActivated                 | bool            |        | bit               |                    |       |                                               |
| ExpireDate                  | DateTime?       |        | datetime          |                    |       |                                               |
| IsSystemAccount             | bool            |        | bit               |                    |       |                                               |
| DateOfJoining               | DateTime?       |        | datetime          |                    |       |                                               |
| DateOfSeparation            | DateTime?       |        | datetime          |                    |       |                                               |
| RfId                        | string?         | 50     | nvarchar          |                    |       |                                               |
| TimeCardNo                  | string?         | 255    | nvarchar          |                    |       |                                               |
| Comment                     | string?         | 2000   | nvarchar          |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: Task

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdTask                      | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdParentTask                | guid?           |        | guid?             | NavigationProperty |       | Self-Reference                                |
| IdUserTaskCreated           | guid?           |        | guid?             | NavigationProperty |       |                                               |
| IdUserAsOwner               | guid?           |        | guid?             | NavigationProperty |       |                                               |
| IdProject                   | guid?           |        | guid?             | NavigationProperty |       |                                               |
| ExternalReferenceId         | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| TaskName                    | string          | 50     | nvarchar          |                    |       |                                               |
| TaskDescription             | string          | 3000   | nvarchar          |                    |       |                                               |
| TaskNo                      | int?            |        | int               |                    |       |                                               |
| TaskOrderNo                 | int             |        | int               |                    |       |                                               |
| DueDate                     | DateTime?       |        | datetime          |                    |       |                                               |
| TaskDone                    | bool            |        | bit               |                    |       |                                               |
| TaskDoneDate                | DateTime?       |        | datetime          |                    |       |                                               |
| TimeInMinutesWorkedOn       | int?            |        | int               |                    |       |                                               |
| PlanedCapacityInMinutes     | int?            |        | int               |                    |       |                                               |
| RememberMinutesBefore       | int?            |        | int               |                    |       |                                               |
| IsTemplate                  | bool            |        | bit               |                    |       |                                               |
| TimePerUnit                 | double?         |        | float             |                    |       |                                               |
| DegreeOfDifficultyProMille  | double?         |        | float             |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: Project

| Property                           | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|------------------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdProject                          | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdUserAsOwner                      | guid?           |        | guid?             | NavigationProperty |       |                                               |
| IdParentProject                    | guid?           |        | guid?             | NavigationProperty |       | Self-Reference                                |
| IdCustomer                         | guid?           |        | guid?             | NavigationProperty |       |                                               |
| ExternalReferenceId                | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| ProjectNumber                      | int             |        | int               |                    |       |                                               |
| ProjectName                        | string          | 255    | nvarchar          |                    |       |                                               |
| ShortProjectName                   | string          | 30     | nvarchar          |                    |       |                                               |
| IsProject                          | bool            |        | bit               |                    |       |                                               |
| IsActive                           | bool            |        | bit               |                    |       |                                               |
| IsSubProject                       | bool            |        | bit               |                    |       |                                               |
| Description                        | string?         | 3000   | nvarchar          |                    |       |                                               |
| MonitorTimeCapacity                | bool?           |        | bit               |                    |       |                                               |
| MonthlyTargetTimeCapacity          | int?            |        | int               |                    |       |                                               |
| MonitorStartdate                   | DateTime?       |        | datetime          |                    |       |                                               |
| MonitorEnddate                     | DateTime?       |        | datetime          |                    |       |                                               |
| TotalTargetTimeCapacity            | int?            |        | int               |                    |       |                                               |
| CollectProductionUnits             | bool            |        | bit               |                    |       |                                               |
| CollectProductionUnitsDescription  | bool            |        | bit               |                    |       |                                               |
| DateLastEdited                     | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                        | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                           | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                          | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |
| HasCustomers                       | bool            |        | bit               |                    |       |                                               |
| MaxBookedEmployees                 | int?            |        | int               |                    |       |                                               |

---

# Entity: Customer

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdCustomer                  | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdCompanyContact            | guid            |        | guid              | NavigationProperty |       |                                               |
| IdMainContact               | guid?           |        | guid?             | NavigationProperty |       |                                               |
| Matchcode                   | string?         | 50     | nvarchar          |                    |       |                                               |
| ExternalReferenceId         | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| CustomerNumber              | int             |        | int               |                    |       |                                               |
| CompanyName                 | string          | 100    | nvarchar          |                    |       |                                               |
| IsIndividual                | bool            |        | bit               |                    |       |                                               |
| IsActive                    | bool            |        | bit               |                    |       | Only active customers appear in selections    |
| Comment                     | string?         | 1000   | nvarchar          |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

# Entity: Vendor

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdVendor                    | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdVendorContact             | guid            |        | guid              | NavigationProperty |       |                                               |
| IdMainContact               | guid?           |        | guid?             | NavigationProperty |       |                                               |
| Matchcode                   | string?         | 50     | nvarchar          |                    |       |                                               |
| ExternalReferenceId         | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| VendorNumber                | int             |        | int               |                    |       |                                               |
| CompanyName                 | string          | 100    | nvarchar          |                    |       |                                               |
| IsIndividual                | bool            |        | bit               |                    |       |                                               |
| IsActive                    | bool            |        | bit               |                    |       | Only active vendors appear in selections      |
| Comment                     | string?         | 1000   | nvarchar          |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: Category

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdCategory                  | guid            |        | guid              | PrimaryIndex       |       |                                               |
| CategoryName                | string          | 200    | nvarchar          |                    |       |                                               |
| CategoryDescription         | string?         | 2000   | nvarchar          |                    |       |                                               |
| IsSystemCategory            | bool            |        | bit               |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: Product

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdProduct                   | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdVendor                    | guid?           |        | guid?             | NavigationProperty |       |                                               |
| ProductName                 | string?         | 1000   | nvarchar          |                    |       |                                               |
| ProductImage                | byte[]?         |        | image             |                    |       |                                               |
| QuantityPerUnit             | decimal?        |        | decimal           |                    |       |                                               |
| UnitDimension               | string?         | 50     | nvarchar          |                    |       |                                               |
| UnitsInStock                | decimal?        |        | decimal           |                    |       |                                               |
| UnitsInProduction           | decimal?        |        | decimal           |                    |       |                                               |
| UnitsAtCustomer             | decimal?        |        | decimal           |                    |       |                                               |
| TeHMin                      | decimal?        |        | decimal           |                    |       |                                               |
| ReturningProductService     | bool?           |        | bit               |                    |       |                                               |
| UnitMainPrice               | decimal?        |        | money             |                    |       |                                               |
| VAT                         | decimal?        |        | decimal           |                    |       |                                               |
| Discontinued                | bool            |        | bit               |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: Contact

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdContact                   | guid            |        | guid              | PrimaryIndex       |       |                                               |
| IdParentContact             | guid?           |        | guid?             | NavigationProperty |       | Self-Reference                                              |
| ExternalReferenceId         | string?         | 128    | nvarchar          | Unique Index       |       |                                               |
| Salutation                  | string?         | 100    | nvarchar          |                    |       |                                               |
| MainName                    | string          | 100    | nvarchar          |                    |       |                                               |
| AdditionalName1             | string?         | 100    | nvarchar          |                    |       |                                               |
| AdditionalName2             | string?         | 100    | nvarchar          |                    |       |                                               |
| Address1                    | string?         | 100    | nvarchar          |                    |       |                                               |
| Address2                    | string?         | 100    | nvarchar          |                    |       |                                               |
| Address3                    | string?         | 100    | nvarchar          |                    |       |                                               |
| Zip                         | string?         | 20     | nvarchar          |                    |       |                                               |
| POBox                       | string?         | 50     | nvarchar          |                    |       |                                               |
| City                        | string?         | 100    | nvarchar          |                    |       |                                               |
| Country                     | string?         | 100    | nvarchar          |                    |       |                                               |
| Email                       | string?         | 100    | nvarchar          |                    |       |                                               |
| PhoneBusiness               | string?         | 100    | nvarchar          |                    |       |                                               |
| PhonePrivate                | string?         | 100    | nvarchar          |                    |       |                                               |
| PhoneMobile                 | string?         | 100    | nvarchar          |                    |       |                                               |
| DateOfBirth                 | DateTime?       |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

---

# Entity: TimeItemType

| Property                    | Type            | MaxLen | ColType           | Index              | Track | Notes                                         |
|-----------------------------|-----------------|--------|-------------------|--------------------|-------|-----------------------------------------------|
| IdTimeItemType              | guid            |        | guid              | PrimaryIndex       |       |                                               |
| TimeItemTypeName            | string          | 50     | nvarchar          |                    |       |                                               |
| ShortName                   | string          | 30     | nvarchar          |                    |       |                                               |
| DisplayOrder                | int?            |        | int               |                    |       |                                               |
| IsSystemType                | bool            |        | bit               |                    |       | System types cannot be deleted                |
| BookingType                 | short           |        | smallint          |                    |       | Maps to TimeItemBookingType enum              |
| Description                 | string          | 255    | nvarchar          |                    |       |                                               |
| DateLastEdited              | DateTime        |        | datetime          |                    |       |                                               |
| DateCreated                 | DateTime        |        | datetime          |                    |       |                                               |
| SyncGuid                    | guid            |        | guid              | Unique             |       |                                               |
| IsDeleted                   | DateTime?       |        | datetime          |                    |       | Soft delete timestamp                         |

## Enums

### TimeItemBookingType Enum

```csharp
/// <summary>
/// Defines the booking type categories for TimeItemTypes.
/// </summary>
public enum TimeItemBookingType : short
{
    /// <summary>
    /// Default/Log entry - used for quantity capture or other non-personnel time tracking events.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Check-in - employee clocks in.
    /// </summary>
    CheckIn = 1,

    /// <summary>
    /// Check-out - employee clocks out.
    /// </summary>
    CheckOut = 2,

    /// <summary>
    /// Break - unspecified break.
    /// </summary>
    Break = 3,

    /// <summary>
    /// Downtime - booked when employee cannot continue working for operational reasons.
    /// </summary>
    Downtime = 4,

    /// <summary>
    /// Business errand - unspecified business errand outside the office.
    /// </summary>
    BusinessErrand = 5,

    /// <summary>
    /// Set booking - booking to a target set that can contain multiple projects, orders, products, or combinations.
    /// </summary>
    SetBooking = 6
}
```

## Default TimeItemType Values

| TimeItemTypeName    | ShortName    | DisplayOrder | IsSystemType | BookingType | Description                                                                                         |
|---------------------|--------------|--------------|--------------|-------------|-----------------------------------------------------------------------------------------------------|
| Default             | Default      | 0            | true         | 0           | Used in the flat time table for all entries related to quantity capture timestamps.                 |
| Check In            | CheckIn      | 1            | true         | 1           | Called when an employee clocks in.                                                                  |
| Check Out           | CheckOut     | 2            | true         | 2           | Called when an employee clocks out.                                                                 |
| Break               | Break        | 3            | true         | 3           | Employee books an unspecified break.                                                                |
| Downtime            | Downtime     | 4            | true         | 4           | Downtime booked when employee cannot continue working for operational reasons.                      |
| Business Errand     | BizErrand    | 5            | true         | 5           | Unspecified business errand outside the office.                                                     |
| Set Booking         | SetBooking   | 6            | true         | 6           | Booking to a target set that can contain multiple projects, orders, products, or combinations.      |
