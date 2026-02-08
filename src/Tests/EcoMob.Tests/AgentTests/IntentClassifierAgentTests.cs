using System.Threading;
using System.Threading.Tasks;
using EcoMob.Infra.Services;
using EcoMob.Contracts.Enums;
using EcoMob.Contracts.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;

namespace EcoMob.Tests.AgentTests
{
    public class IntentClassifierAgentTests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EmptyPrompt_ReturnsUnknown()
        {
            var mockChat = new Moq.Mock<IChatClient>();
            var logger = new NullLogger<IntentClassifierAgent>();
            var agent = new IntentClassifierAgent(mockChat.Object, logger);

            var result = await agent.ClassifyAsync("", CancellationToken.None);

            Assert.Equal(IntentType.UNKNOWN, result.Type);
        }

    }
}