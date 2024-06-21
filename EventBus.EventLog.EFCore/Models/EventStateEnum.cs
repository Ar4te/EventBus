namespace EventBus.EventLog.EFCore.Models;

public enum EventStateEnum
{
    NotPublished = 0,
    InProgress,
    Published,
    PublishedFailed
}
