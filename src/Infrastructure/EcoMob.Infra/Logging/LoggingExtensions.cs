using Microsoft.Extensions.Logging;

namespace EcoMob.Infra.Logging
{
    /// <summary>
    /// Small helper to create consistent logging scopes for agents and components.
    /// </summary>
    public static class LoggingExtensions
    {
        public static IDisposable BeginAgentScope(this ILogger logger, string agentName)
        {
            if (logger == null) return NullLoggerDisposable.Instance;
            if (agentName == null) return logger.BeginScope(new Dictionary<string, object?> { ["Agent"] = "unknown" });
            return logger.BeginScope(new Dictionary<string, object?> { ["Agent"] = agentName });
        }

        private sealed class NullLoggerDisposable : IDisposable
        {
            public static readonly NullLoggerDisposable Instance = new();
            public void Dispose() { }
        }
    }
}