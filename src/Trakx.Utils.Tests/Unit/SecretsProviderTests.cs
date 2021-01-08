using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Testing;
using Xunit;
using static System.Environment;

namespace Trakx.Utils.Tests.Unit
{
    public class SecretsProviderTests
    {
        [Fact]
        public void Set_Property_Value_For_Secrets_Class()
        {
            var expectedResult = "env_var_value";
            SetEnvironmentVariable("env_var_name", expectedResult);

            var provider = new SecretsProvider<TestSecrets>();
            var envVarValue = provider.GetSecrets();

            envVarValue
                .EnvironmentVar
                .Should()
                .Equals(expectedResult);
        }

        [Fact]
        public void Return_Empty_Secrets_Class()
        {
            var provider = new SecretsProvider<TestSecrets>();

            var envVarValue = provider.GetSecrets();

            envVarValue
                .EnvironmentVar
                .Should()
                .Equals(string.Empty);
        }

        private class TestSecrets
        {
            [ReadmeDocument("env_var_name")]
            public string EnvironmentVar { get; set; }
        }
    }
}
