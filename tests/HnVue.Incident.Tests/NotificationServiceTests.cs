using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Incident;
using HnVue.Incident.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HnVue.Incident.Tests;

/// <summary>
/// Tests for <see cref="NotificationService"/>.
/// NSubstitute cannot proxy ILogger{T} when T is internal and the logging assembly is strong-named.
/// Tests therefore use a <see cref="SpyLoggerProvider"/> to capture log records instead.
/// </summary>
public sealed class NotificationServiceTests
{
    private static IncidentRecord MakeRecord(IncidentSeverity severity)
        => new(
            IncidentId: Guid.NewGuid().ToString(),
            OccurredAt: DateTimeOffset.UtcNow,
            ReportedByUserId: "user-01",
            Severity: severity,
            Category: "TEST_CAT",
            Description: "Test description",
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

    private static (NotificationService Sut, SpyLoggerProvider Spy) BuildSut()
    {
        var spy = new SpyLoggerProvider();
        using var factory = LoggerFactory.Create(b => b.AddProvider(spy).SetMinimumLevel(LogLevel.Trace));
        var logger = factory.CreateLogger<NotificationService>();
        return (new NotificationService(logger), spy);
    }

    [Fact]
    public void Notify_CriticalSeverity_LogsAtErrorLevel()
    {
        var (sut, spy) = BuildSut();

        sut.Notify(MakeRecord(IncidentSeverity.Critical));

        spy.Records.Should().ContainSingle(r => r.Level == LogLevel.Error);
    }

    [Fact]
    public void Notify_HighSeverity_LogsAtWarningLevel()
    {
        var (sut, spy) = BuildSut();

        sut.Notify(MakeRecord(IncidentSeverity.High));

        spy.Records.Should().ContainSingle(r => r.Level == LogLevel.Warning);
    }

    [Fact]
    public void Notify_MediumSeverity_LogsAtInformationLevel()
    {
        var (sut, spy) = BuildSut();

        sut.Notify(MakeRecord(IncidentSeverity.Medium));

        spy.Records.Should().ContainSingle(r => r.Level == LogLevel.Information);
    }

    [Fact]
    public void Notify_LowSeverity_LogsAtDebugLevel()
    {
        var (sut, spy) = BuildSut();

        sut.Notify(MakeRecord(IncidentSeverity.Low));

        spy.Records.Should().ContainSingle(r => r.Level == LogLevel.Debug);
    }

    [Fact]
    public void Notify_DoesNotThrow_WhenNullLogger()
    {
        var sut = new NotificationService(NullLogger<NotificationService>.Instance);

        var act = () => sut.Notify(MakeRecord(IncidentSeverity.Critical));

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(IncidentSeverity.Critical)]
    [InlineData(IncidentSeverity.High)]
    [InlineData(IncidentSeverity.Medium)]
    [InlineData(IncidentSeverity.Low)]
    public void Notify_AllSeverities_LogExactlyOnce(IncidentSeverity severity)
    {
        var (sut, spy) = BuildSut();

        sut.Notify(MakeRecord(severity));

        spy.Records.Should().HaveCount(1, $"exactly one log entry per Notify call is expected for {severity}");
    }
}

// ── Test infrastructure ────────────────────────────────────────────────────────

internal sealed class SpyLoggerProvider : ILoggerProvider
{
    public List<LogRecord> Records { get; } = [];

    public ILogger CreateLogger(string categoryName) => new SpyLogger(Records);

    public void Dispose() { }
}

internal sealed record LogRecord(LogLevel Level, string Message);

internal sealed class SpyLogger(List<LogRecord> records) : ILogger
{
    private readonly List<LogRecord> _records = records;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _records.Add(new LogRecord(logLevel, formatter(state, exception)));
    }
}
