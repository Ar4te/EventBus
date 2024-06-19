namespace EventBus.EventLog.Npgsql.Models;

public enum EventStateEnum
{
    NotPublished = 0,
    InProgress,
    Published,
    PublishedFailed
}
