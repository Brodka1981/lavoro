using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Utils;

public static class Log
{
    // Critical
    private static readonly Action<ILogger, Exception?, string, object?[]> _criticalLogger = LoggerExtensions.LogCritical;
    public static void LogCritical(ILogger logger, string message, params object?[] args) => _criticalLogger(logger, null!, message, args);

    // Error
    private static readonly Action<ILogger, Exception?, string, object?[]> _errorLogger = LoggerExtensions.LogError;
    public static void LogError(ILogger logger, string message, params object?[] args) => _errorLogger(logger, null!, message, args);
    public static void LogError(ILogger logger, string message, Exception? exception = null, params object?[] args) => _errorLogger(logger, exception, message, args);

    // Warning
    private static readonly Action<ILogger, Exception?, string, object?[]> _warningLogger = LoggerExtensions.LogWarning;
    public static void LogWarning(ILogger logger, string message, params object?[] args) => _warningLogger(logger, null!, message, args);

    //Info    
    private static readonly Action<ILogger, Exception?, string, object?[]> _infoLogger = LoggerExtensions.LogInformation;
    public static void LogInfo(ILogger logger, string message, params object?[] args) => _infoLogger(logger, null!, message, args);

    // Trace
    private static readonly Action<ILogger, Exception?, string, object?[]> _traceLogger = LoggerExtensions.LogTrace;
    public static void LogTrace(ILogger logger, string message, params object?[] args) => _traceLogger(logger, null!, message, args);

    // Debug
    private static readonly Action<ILogger, Exception?, string, object?[]> _debugLogger = LoggerExtensions.LogDebug;
    public static void LogDebug(ILogger logger, string message, params object?[] args) => _debugLogger(logger, null!, message, args);

    private static readonly Func<ILogger, string, string, IDisposable?> _beginScopeSaga = LoggerMessage.DefineScope<string, string>("ConversationId: {ConversationId} SagaId: {SagaId}");
    private static readonly Func<ILogger, string, IDisposable?> _beginScope = LoggerMessage.DefineScope<string>("ConversationId: {ConversationId}");

    public static IDisposable? BeginScope(ILogger logger, string conversationId, string sagaId) => _beginScopeSaga(logger, conversationId, sagaId);
    public static IDisposable? BeginScope(ILogger logger, string conversationId) => _beginScope(logger, conversationId);
}
