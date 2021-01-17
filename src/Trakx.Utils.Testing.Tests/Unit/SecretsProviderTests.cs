using FluentAssertions;
using Trakx.Utils.Attributes;
using Xunit;
using static System.Environment;

namespace Trakx.Utils.Testing.Tests.Unit
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
                .Be(expectedResult);
        }

        [Fact]
        public void Return_Empty_Secrets_Class()
        {
            var provider = new SecretsProvider<TestSecrets>();

            var envVarValue = provider.GetSecrets();

            envVarValue
                .EnvironmentVar
                .Should()
                .BeNull();
        }

        private class TestSecrets
        {
            [SecretEnvironmentVariable("env_var_name")]
            public string? EnvironmentVar { get; set; }
        }
    }
}
