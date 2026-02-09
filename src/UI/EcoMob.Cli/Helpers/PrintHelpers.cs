namespace EcoMob.Cli.Helpers
{
    public static class PrintHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void PrintEcoMobMessage(string message)
        {
            Console.WriteLine($"\n🤖 EcoMob > {message}");
        }


        /// <summary>
        /// 
        /// </summary>
        public static void PrintGoodbye()
        {
            Console.WriteLine("\n👋 EcoMob > Goodbye! Move green 🌿");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsExitCommand(string input) =>
            input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        public static void PrintWelcome()
        {
            Console.WriteLine("""
            🌱 EcoMobility Assistant
            ----------------------------------
            Ask me about parking, routes, EV charging,
            public transport, and sustainable mobility.
            Type 'exit' to quit.
            """);
        }
    }
}
