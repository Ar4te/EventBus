﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using EventBus.Events;

namespace EventBus.EventLog.FreeSql.Models;

public class IntegrationEventLogEntry
{
    private static readonly JsonSerializerOptions _indentedOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions _caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };
    private IntegrationEventLogEntry() { }

    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
    {
        EventId = @event.Id;
        CreationTime = @event.CreationDate;
        EventTypeName = @event.GetType().FullName!;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), _indentedOptions);
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId;
    }

    public Guid EventId { get; private set; }

    [Required] public string EventTypeName { get; private set; }

    [NotMapped] public string EventTypeShortName => EventTypeName.Split('.')?.Last() ?? "";

    [NotMapped] public IntegrationEvent IntegrationEvent { get; private set; }

    public EventStateEnum State { get; set; }

    public int TimesSent { get; set; }

    public DateTime CreationTime { get; private set; }

    [Required] public string Content { get; private set; }

    public Guid TransactionId { get; private set; }

    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        IntegrationEvent = (JsonSerializer.Deserialize(Content, type, _caseInsensitiveOptions) as IntegrationEvent)!;
        return this;
    }
}
