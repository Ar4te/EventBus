using System.Reflection;
using EventBus.EventLog.Npgsql.Models;
using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventBus.EventLog.Npgsql.Services;

public class IntegrationEventLogService<TDbContext> : IIntegrationEventLogService, IDisposable
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly Type[] _eventTypes;
    private volatile bool _disposedValue;

    public IntegrationEventLogService(TDbContext dbContext)
    {
        _dbContext = dbContext;
        _eventTypes = Assembly.Load(Assembly.GetEntryAssembly()?.FullName!)
            .GetTypes()
            .Where(t => t.Name.EndsWith(nameof(IntegrationEvent)))
            .ToArray();
    }

    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var pendingEvents = await _dbContext.Set<IntegrationEventLogEntry>()
            .Where(t => t.TransactionId == transactionId && t.State == EventStateEnum.NotPublished)
            .ToListAsync();
        if (pendingEvents.Count != 0)
        {
            return pendingEvents.OrderBy(o => o.CreationTime)
                .Select(o => o.DeserializeJsonContent(_eventTypes.FirstOrDefault(t => t.Name == o.EventTypeShortName)!));
        }
        return [];
    }

    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);

        _dbContext.Database.UseTransaction(transaction.GetDbTransaction());
        _dbContext.Set<IntegrationEventLogEntry>().Add(eventLogEntry);

        return _dbContext.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    private Task<int> UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        var eventLogEntry = _dbContext.Set<IntegrationEventLogEntry>()
            .SingleOrDefault(e => e.EventId == eventId);

        if (eventLogEntry == null) return Task.FromResult(0);

        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
        {
            eventLogEntry.TimesSent++;
        }

        return _dbContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
