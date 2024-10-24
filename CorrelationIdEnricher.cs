using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (Activity.Current?.Id != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "CorrelationId", Activity.Current.Id));
        }
    }
}