using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using EcoMob.Infra.Extensions;
using EcoMob.Infra.Settings;

namespace EcoMob.Tests.ArchTests
{
    public class InfraLayerConfigTests
    {
        /// <summary>
        /// Tests that the infrastructure layer throws an exception when the OpenAI key is missing.
        /// </summary>
        [Fact]
        public void AddInfrastructureLayer_Throws_When_OpenAIKeyMissing()
        {
            var inMemory = new Dictionary<string, string?>
            {
                ["McpSettings:TransportType"] = "stdio",
                ["McpSettings:OpenAIKey"] = ""
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var services = new ServiceCollection();

            services.AddInfrastructureLayer(config);

            var provider = services.BuildServiceProvider();

            // Accessing the options value should trigger validation
            Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<McpSettings>>().Value);
        }
    }
}