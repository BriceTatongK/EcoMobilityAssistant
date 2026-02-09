using EcoMob.Contracts.Services;
using EcoMob.Infra.Services;
using NetArchTest.Rules;

namespace EcoMob.ArchitectureTests
{
    public class DependencyRulesTests
    {
        private const string Cli = "EcoMob.Cli";
        private const string Core = "EcoMob.Core";
        private const string Contracts = "EcoMob.Contracts";
        private const string Infrastructure = "EcoMob.Infrastructure";


        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Core_Should_Not_Depend_On_Infrastructure_Or_Cli()
        {
            var result = Types
                .InAssembly(typeof(IReasoningAgent).Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(Infrastructure, Cli)
                .GetResult();

            AssertSuccessful(result, GetFailMessage(Core));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Infrastructure_Should_Not_Depend_On_Core_Or_Cli()
        {
            var result = Types
                .InAssembly(typeof(ReasoningAgent).Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(Core, Cli)
                .GetResult();

            AssertSuccessful(result, GetFailMessage(Infrastructure));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Contracts_Should_Not_Depend_On_Any_Application_Code()
        {
            var result = Types
                .InAssembly(typeof(IReasoningAgent).Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(Core, Infrastructure, Cli)
                .GetResult();

            AssertSuccessful(result, GetFailMessage(Contracts));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="message"></param>
        private void AssertSuccessful(TestResult result, string message = "")
        {
            if (!result.IsSuccessful)
            {
                var failingTypes = string.Join(Environment.NewLine, result.FailingTypeNames);

                Assert.Fail($"{message}\nFailing types:\n{failingTypes}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private string GetFailMessage(string component)
        {
            return $"{component} has forbidden dependencies on other projects: ";
        }
    }
}
