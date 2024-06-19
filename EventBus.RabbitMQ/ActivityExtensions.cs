using System.Diagnostics;

namespace EventBus.RabbitMQ;

public static class ActivityExtensions
{
    public static void SetExceptionTags(this Activity activity, Exception exception)
    {
        if (activity == null) return;
        activity.AddTag("exception.message", exception.Message);
        activity.AddTag("exception.stacktrace", exception.StackTrace);
        activity.AddTag("exception.type", exception.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
