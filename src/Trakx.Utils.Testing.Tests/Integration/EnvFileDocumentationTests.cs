using Xunit.Abstractions;

namespace Trakx.Utils.Testing.Tests.Integration
{
    /// <summary>
    /// This template should be added to test suites of projects which need
    /// secrets to be provided by environment variables. It will run an inherited test
    /// that creates a .env file template.
    /// </summary>
    /// <remarks>This will not work if being added to several projects in the same solution, as
    /// one run will create a file the overwrites the ones created by other runs.</remarks>
    public class EnvFileDocumentationTests : EnvFileDocumentationCreator
    {
        public EnvFileDocumentationTests(ITestOutputHelper output) : base(output) { }
    }
}