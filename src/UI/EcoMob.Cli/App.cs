using EcoMob.Core.Services;
using EcoMob.Contracts.Services;
using Microsoft.Extensions.Logging;

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
            PrintWelcome();

            while (true)
            {
                Console.Write("\n🌍 You > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (IsExitCommand(input))
                    break;

                try
                {
                    // Step 1: Validate request relevance
                    var validation = await _validatorAgent.ValidateAsync(input);

                    if (!validation.IsValid)
                    {
                        PrintEcoMobMessage(
                            $"I can help with eco-mobility topics only 🌱\n→ {validation.Reason}"
                        );
                        continue;
                    }

                    // Step 2: Process request
                    var response = await _ecoMobilityService.HandleUserRequestAsync(input);
                    PrintEcoMobMessage(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error while processing user input");

                    PrintEcoMobMessage(
                        "Something went wrong while processing your request. Please try again."
                    );
                }
            }

            PrintGoodbye();
        }

        private static bool IsExitCommand(string input) =>
            input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase);

        private static void PrintWelcome()
        {
            Console.WriteLine("""
            🌱 EcoMobility Assistant
            ----------------------------------
            Ask me about parking, routes, EV charging,
            public transport, and sustainable mobility.
            Type 'exit' to quit.
            """);
        }

        private static void PrintEcoMobMessage(string message)
        {
            Console.WriteLine($"\n🤖 EcoMob > {message}");
        }

        private static void PrintGoodbye()
        {
            Console.WriteLine("\n👋 EcoMob > Goodbye! Move green 🌿");
        }
    }
}
