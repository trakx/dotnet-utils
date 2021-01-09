using System.Threading.Tasks;
using FluentAssertions;
using Trakx.Utils.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Tests.Integration
{
    public class EnvFileDocumentationTests
    {
        private readonly ITestOutputHelper _output;

        public EnvFileDocumentationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task MyTestedMethod_should_be_producing_this_result_when_some_conditions_are_met()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation();
            envFileDocCreated.Should().BeTrue();
        }
    }
}