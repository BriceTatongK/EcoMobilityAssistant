using EcoMob.Core.Services;
using EcoMob.Contracts.Services;
using Microsoft.Extensions.Logging;
using EcoMob.Cli.Helpers;

namespace EcoMob.Cli
{
    internal class App(
        IValidatorAgent validatorAgent,
        EcoMobilityService ecoMobilityService,
        ILogger<App> logger)
    {
        private readonly ILogger<App> _logger = logger;
        private readonly IValidatorAgent _validatorAgent = validatorAgent;
        private readonly EcoMobilityService _ecoMobilityService = ecoMobilityService;

        public async Task RunAsync()
        {
            PrintHelpers.PrintWelcome();

            while (true)
            {
                Console.Write("\n🌍 You > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (PrintHelpers.IsExitCommand(input))
                    break;

                try
                {
                    // Step 1: Validate request relevance
                    var validation = await _validatorAgent.ValidateAsync(input);

                    if (!validation.IsValid)
                    {
                        PrintHelpers.PrintEcoMobMessage(
                            $"I can help with eco-mobility topics only 🌱\n→ {validation.Reason}"
                        );
                        continue;
                    }

                    // Step 2: Process request
                    var response = await _ecoMobilityService.HandleUserRequestAsync(input);
                    PrintHelpers.PrintEcoMobMessage(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error while processing user input");

                    PrintHelpers.PrintEcoMobMessage(
                        "Something went wrong while processing your request. Please try again."
                    );
                }
            }

            PrintHelpers.PrintGoodbye();
        }
    }
}
