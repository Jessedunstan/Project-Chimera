namespace Chimera.Core.Contracts;

/// <summary>
/// Contract for recording significant events to persistent world ledgers and historical logs.
/// </summary>
public interface IHistoricalEventPublisher
{
    void RecordEvent(string eventType, string summary, long worldTimestamp);
}
