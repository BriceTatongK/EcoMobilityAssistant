using System.Threading;
using System.Threading.Tasks;
using EcoMob.Infra.Services;
using EcoMob.Contracts.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;

namespace EcoMob.Tests.AgentTests
{
    public class ContextValidatorAgentTests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EmptyPrompt_ReturnsInvalid()
        {
            var mockChat = new Moq.Mock<IChatClient>();
            var logger = new NullLogger<ContextValidatorAgent>();
            var agent = new ContextValidatorAgent(mockChat.Object, logger);

            var result = await agent.ValidateAsync("", CancellationToken.None);

            Assert.False(result.IsValid);
        }

    }
}