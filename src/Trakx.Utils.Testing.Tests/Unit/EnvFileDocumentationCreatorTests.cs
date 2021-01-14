using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing.Tests.Unit
{
    public class EnvFileDocumentationCreatorTests
    {
        public readonly ITestOutputHelper _output;

        public EnvFileDocumentationCreatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // no environment vars and no md should do nothing
        [Fact]
        public async Task NoEnvironmentVariablesWithoutMdFile()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

        // no environment vars and md with no previous entry should do nothing
        [Fact]
        public async Task NoEnvironmentVariablesWithMdFileNotIncludingShouldDoNothing()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";
            creator.MdFileName = "ThisFileShouldNotExists.md";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

        // no environment vars and md with previous entry should delete lines
        [Fact]
        public async Task NoEnvironmentVariablesWithMdFileIncludingShouldDelete()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";
            creator.MdFileName = "ThisFileShouldNotHaveEnvVars.md";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

        // environment vars and no md should create new file with lines
        [Fact]
        public async Task EnvironmentVariablesWithoutMdFileShouldCreate()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";
            creator.MdFileName = "ThisFileShouldBeCreated.md";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

        // environment vars and md with no previous should insert lines
        [Fact]
        public async Task EnvironmentVariablesWitMdFileNotIncludingShouldInsert()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";
            creator.MdFileName = "ThisFileShouldHaveNewLines.md";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

        // environment vars and md with previous should update lines
        [Fact]
        public async Task EnvironmentVariablesWitMdFileIncludingShouldUupdate()
        {
            var creator = new EnvFileDocumentationCreator(_output);
            creator.DocumentedProjectName = "Trakx.Utils.Testing";
            creator.MdFileName = "ThisFileShouldBeFixed.md";

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
        }

    }
}
