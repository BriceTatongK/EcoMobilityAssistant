using System.Threading;
using System.Threading.Tasks;
using EcoMob.Infra.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;
using McpDotNet.Client;
using McpDotNet.Protocol.Types;
using Xunit;
using System.Collections.Generic;
using Moq;
using EcoMob.Contracts.Models;

namespace EcoMob.Tests.AgentTests
{
    public class ReasoningAgentTests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EmptyPrompt_ReturnsEmpty()
        {
            var mockChat = new Moq.Mock<IChatClient>();
            var mockMcp = new Moq.Mock<McpDotNet.Client.IMcpClient>();
            var logger = new NullLogger<ReasoningAgent>();
            var agent = new ReasoningAgent(mockMcp.Object, mockChat.Object, logger);

            var result = await agent.ProcessAsync(new IntentContext(), "", CancellationToken.None);

            Assert.Equal(string.Empty, result);
        }

        private static async IAsyncEnumerable<McpDotNet.Protocol.Types.Tool> GetEmptyTools()
        {
            yield break;
        }
    }
}
