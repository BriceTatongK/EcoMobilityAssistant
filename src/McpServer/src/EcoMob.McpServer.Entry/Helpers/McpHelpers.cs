using McpDotNet.Protocol.Types;
using System.Text.Json;

namespace EcoMob.McpServer.Entry.Helpers
{
    public static class McpResponse
    {
        public static CallToolResponse Ok(object data) =>
            new()
            {
                IsError = false,
                Content =
                [
                    new Content
                {
                    Type = "text",
                    Text = JsonSerializer.Serialize(data)
                }
                ]
            };

        public static CallToolResponse Error(string message) =>
            new()
            {
                IsError = true,
                Content =
                [
                    new Content { Type = "text", Text = message }
                ]
            };
    }

}
