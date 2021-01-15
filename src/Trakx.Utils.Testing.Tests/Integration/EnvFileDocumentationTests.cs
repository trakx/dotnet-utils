using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Trakx.Utils.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing.Tests.Integration
{
    public class FakeConfiguration
    {
        [SecretEnvironmentVariable("SecretAbc")]
        public string SecretString { get; set; }

        [SecretEnvironmentVariable("Secret123")]
        public int SecretNumber { get; set; }

        private string NotSecret { get; set; }

    }

    public class EnvFileDocumentationUpdaterTests
    {
        private readonly ITestOutputHelper _output;
        private readonly EnvFileDocumentationUpdater _updater;
        private readonly IReadmeEditor _readmeEditor;

        public EnvFileDocumentationUpdaterTests(ITestOutputHelper output)
        {
            _output = output;

            _readmeEditor = Substitute.For<IReadmeEditor>();
            _updater = new EnvFileDocumentationUpdater(_output, _readmeEditor);
        }

        [Fact]
        public async Task UpdateEnvFileDocumentation_should_not_update_when_section_does_not_exist()
        {
            _readmeEditor.ExtractReadmeContent(null).ReturnsForAnyArgs(
                "## Existing Section" + Environment.NewLine +
                "with a paragraph, and some text" + Environment.NewLine);

            var success = await _updater.UpdateEnvFileDocumentation();
            success.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateEnvFileDocumentation_should_update_when_section_exist()
        {
            var existingSecret = "FakeConfiguration__SecretAbc=********";
            var secretToBeAdded = "FakeConfiguration__Secret123=********";

            var textToKeep = 
                "## Existing Section" + Environment.NewLine +
                "with a paragraph, and some text" + Environment.NewLine +
                Environment.NewLine + Environment.NewLine +
                "## Creating your local .env file" + Environment.NewLine +
                "In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:" + Environment.NewLine +
                "```secretsEnvVariables" + Environment.NewLine;
            var readmeContent =
                textToKeep +
                existingSecret + Environment.NewLine +
                "```" + Environment.NewLine;
            
            _readmeEditor.ExtractReadmeContent(null).ReturnsForAnyArgs(
                readmeContent);

            var success = await _updater.UpdateEnvFileDocumentation();
            success.Should().BeTrue();

            await _readmeEditor.Received(1).UpdateReadmeContent(Arg.Any<string>(), Arg.Any<string>());
            var firstArgument = _readmeEditor.ReceivedCalls()
                .Single(c => c.GetMethodInfo().Name == nameof(_readmeEditor.UpdateReadmeContent)).GetArguments()[1] as string;
            firstArgument.Should().Contain(secretToBeAdded);
        }
    }
}