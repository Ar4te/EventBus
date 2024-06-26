﻿using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace EventBus.RabbitMQ;

public class RabbitMQTelemetry
{
    public static string ActivitySourceName = "EventBusRabbitMQ";

    public ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);
    public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;
}
