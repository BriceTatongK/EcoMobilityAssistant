namespace EcoMob.McpServer.Contracts.Models
{
    public class MobilityApiClientFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }

    public class MobilityServiceFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }
}
