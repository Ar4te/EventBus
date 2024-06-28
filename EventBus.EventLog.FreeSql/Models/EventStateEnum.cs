namespace EventBus.EventLog.FreeSql.Models;

public enum EventStateEnum
{
    NotPublished = 0,
    InProgress,
    Published,
    PublishedFailed
}
