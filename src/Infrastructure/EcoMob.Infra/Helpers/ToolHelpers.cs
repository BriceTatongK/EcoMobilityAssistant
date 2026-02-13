using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace EcoMob.Infra.Helpers
{
    public static class ToolHelpers
    {
        /// <summary>
        /// Formats the given tool into a readable string representation.
        /// </summary>
        /// <param name="t">The tool to format.</param>
        /// <returns>A formatted string representing the tool.</returns>
        public static string FormatTool(McpClientTool t)
        {
            string schemaJson = t.JsonSchema.ValueKind != JsonValueKind.Undefined
                                ? t.JsonSchema.GetRawText()
                                : "{}";

            return
                    $@"- TOOL NAME: {t.Name}
                      Title: {t.Title}
                      Description: {t.Description}
                      Input Schema:
                      {schemaJson}
                    ";
        }
    }
}
