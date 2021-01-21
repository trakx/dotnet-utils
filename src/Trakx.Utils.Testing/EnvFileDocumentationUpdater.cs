using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing
{
    public interface IReadmeEditor
    {
        Task<string> ExtractReadmeContent(string filePath);
        Task UpdateReadmeContent(string filePath, string newContent);
    }

    public class ReadmeEditor : IReadmeEditor
    {
        public async Task<string> ExtractReadmeContent(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }

        public async Task UpdateReadmeContent(string filePath, string newContent)
        {
            await File.WriteAllTextAsync(filePath, newContent);
        }
    }

    /// <summary>
    /// This class should be inherited in the est suites of projects which need
    /// secrets to be provided by environment variables. It will run trigger the run
    /// of <see cref="TryUpdateEnvFileDocumentation_should_create_env_template_file"/>
    /// that updates .env file template.
    /// </summary>
    public abstract class EnvFileDocumentationUpdaterBase
    {
        private readonly ITestOutputHelper _output;
        private readonly IReadmeEditor _editor;
        protected IReadmeEditor Editor => _editor;
        private static readonly Regex DotEnvSection = new Regex(@"```secretsEnvVariables\r?\n(?<envVars>(?<envVar>([\w]+)=(\*)+\r?\n)+)```\r?\n");
        private readonly PathAssemblyResolver _resolver;
        protected readonly Assembly ImplementingAssembly;

        protected EnvFileDocumentationUpdaterBase(ITestOutputHelper output, IReadmeEditor? editor = default)
        {
            _output = output;
            _editor = editor ?? new ReadmeEditor();
            _resolver = GetTrakxAssemblyResolver();
            ImplementingAssembly = this.GetType().Assembly;
        }

        [Fact]
        public async Task TryUpdateEnvFileDocumentation_should_create_env_template_file()
        {
            var envFileDocCreated = await UpdateEnvFileDocumentation().ConfigureAwait(false);
            envFileDocCreated.Should().BeTrue();
        }

        public async Task<bool> UpdateEnvFileDocumentation()
        {
            try
            {
                var expectedEnvVarSecrets = GetExpectedEnvVarSecretsFromLoadedAssemblies();
                if (!expectedEnvVarSecrets.Any()) return true;

                var readmeDirectoryInfo = GetRepositoryRootDirectory();
                var readmeFilePath = Path.Combine(readmeDirectoryInfo!.FullName, "README.md");

                var readmeContent = await _editor.ExtractReadmeContent(readmeFilePath);
                var secretsMentionedInReadme = DotEnvSection.Match(readmeContent);

                if (!secretsMentionedInReadme.Success)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Your README.md file should contain the following section:");
                    AppendExampleEnvFileDocumentationSection(stringBuilder, expectedEnvVarSecrets);
                    _output.WriteLine(stringBuilder.ToString());
                    return false;
                }

                var contentToReplace = secretsMentionedInReadme.Groups["envVars"].Value;
                var newContent = string.Join(Environment.NewLine, GetExpectedEnvVarSecretsFromLoadedAssemblies())! + Environment.NewLine;
                var newReadmeContent = readmeContent.Replace(contentToReplace, newContent, StringComparison.InvariantCulture);

                await _editor.UpdateReadmeContent(readmeFilePath, newReadmeContent);

                return true;
            }
            catch (Exception e)
            {
                _output.WriteLine($"Failed to update env file documentation.");
                _output.WriteLine(e.ToString());
                return false;
            }
        }

        private DirectoryInfo GetRepositoryRootDirectory()
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            if (!directory.TryWalkBackToRepositoryRoot(out var repositoryRoot))
                repositoryRoot.Should().NotBeNull("Tests should be running from its repository root.");
            _output.WriteLine($"using {repositoryRoot!.FullName} as repository root path.");
            return repositoryRoot;
        }

        private static void AppendEnvVarNamesByAssembly(StringBuilder builder, List<string> expectedEnvVars)
        {
            foreach (var envVar in expectedEnvVars)
            {
                builder.AppendLine(envVar);
            }
            builder.AppendLine(string.Empty);
        }

        private static void AppendExampleEnvFileDocumentationSection(StringBuilder builder, List<string> expectedEnvVars)
        {
            builder.AppendLine("## Creating your local .env file");
            builder.AppendLine("In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:");
            builder.AppendLine("```secretsEnvVariables");
            AppendEnvVarNamesByAssembly(builder, expectedEnvVars);
            builder.AppendLine("```");
            builder.AppendLine(string.Empty);
        }

        private List<string> GetExpectedEnvVarSecretsFromLoadedAssemblies()
        {
            var assemblies = LoadReferencedAssembliesMetadata()
                .Where(a => a.FullName?.StartsWith("Trakx.") ?? false);
            var explorationContext = new AssemblyLoadContext(nameof(EnvFileDocumentationUpdaterBase), true);

            var result = assemblies.SelectMany(currentAssembly =>
                {
                    _output.WriteLine("Inspecting assembly {0}", currentAssembly.FullName);
                    var assemblyTypes = currentAssembly.GetTypes();
                    if (!assemblyTypes.Any(t => t.FullName?.EndsWith("Configuration") ?? false))
                        return Enumerable.Empty<string>();
                    var fullyLoadedAssembly = explorationContext.LoadFromAssemblyPath(currentAssembly.Location);
                    var configTypes = fullyLoadedAssembly.GetTypes().Where(t => t.FullName?.EndsWith("Configuration") ?? false);
                    var secrets = configTypes
                        .SelectMany(t => t.GetProperties()
                            .Select(p =>
                                p.GetCustomAttribute(typeof(SecretEnvironmentVariableAttribute)) is SecretEnvironmentVariableAttribute attribute
                                    ? (attribute.VarName ?? $"{t.Name}__{p.Name}")+"=********"
                                    : null))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                    return secrets.Select(s => s!);
                })
                .Distinct()
                .OrderBy(abc => abc)
                .ToList();

            explorationContext.Unload();

            return result;
        }

        private List<Assembly> LoadReferencedAssembliesMetadata(int maxRecursions = 10)
        {
            var explorationContext = new MetadataLoadContext(_resolver);

            var knownAssemblies = explorationContext.GetAssemblies().Union(new[] { ImplementingAssembly }).ToList();
            var recursions = 0;

            List<string> newAssemblyNames;
            do
            {
                recursions++;

                var knownNames = knownAssemblies.Select(a => a.GetName().FullName).Distinct().ToList();
                var referencedNames = knownAssemblies.SelectMany(
                    a => a.GetReferencedAssemblies().Where(n => n.FullName.StartsWith("Trakx")))
                    .Select(a => a.FullName);
                newAssemblyNames = referencedNames.Except(knownNames).ToList();
                foreach (var name in newAssemblyNames)
                {
                    try
                    {
                        knownAssemblies.Add(explorationContext.LoadFromAssemblyName(name));
                    }
                    catch (Exception e) { _output.WriteLine("Failed to load assembly {0} with exception {1}", name, e); }
                }

            } while (newAssemblyNames.Any() && recursions < maxRecursions);

            return knownAssemblies;
        }

        private static PathAssemblyResolver GetTrakxAssemblyResolver()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var executingAssemblyLocation = new FileInfo(executingAssembly!.Location).Directory!.GetFiles("Trakx.*.dll")
                .Select(f => f.FullName).ToList();
            var runtimeAssembliesLocations =
                Directory.GetFiles(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

            var assemblyPaths = runtimeAssembliesLocations.Union(executingAssemblyLocation);
            var resolver = new PathAssemblyResolver(assemblyPaths);
            return resolver;
        }
    }
}
