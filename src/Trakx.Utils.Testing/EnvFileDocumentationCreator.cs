using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing
{
    public class EnvFileDocumentationCreator
    {
        private readonly ITestOutputHelper _output;

        public string? DocumentedProjectName { get; set; }
        public string MdFileName { get; set; }

        public EnvFileDocumentationCreator(ITestOutputHelper output)
        {
            _output = output;
            MdFileName = "Environment.md";
        }

        [Fact]
        public async Task TryCreateEnvFileDocumentation_should_create_env_template_file()
        {
            var creator = new EnvFileDocumentationCreator(_output);

            var envFileDocCreated = await creator.TryCreateEnvFileDocumentation().ConfigureAwait(false);
            envFileDocCreated.Should().BeTrue();
        }

        public async Task<bool> TryCreateEnvFileDocumentation()
        {
            try
            {
                var directory = GetAssemblyDirectory();

                if (!directory.TryWalkBackToRepositoryRoot(out var repositoryRoot))
                    repositoryRoot.Should().NotBeNull("Tests should be running from its repository root.");

                _output.WriteLine($"using {repositoryRoot!.FullName} as repository root path.");

                var readmeFilePath = Path.Combine(repositoryRoot!.FullName, MdFileName);
                var environmentVariables = GetEnvironmentVariableNames();

                var mdLines = new List<string>();

                DumpVarNamesByAssembly(mdLines, environmentVariables);

                if (!File.Exists(readmeFilePath))
                {
                    if (environmentVariables.Any())
                    {
                        InsertHeaderLines(mdLines);
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    var currentFileLines = await ReadCurrentFileLinesAsync(readmeFilePath);
                    ReplaceLinesInCurrentFile(currentFileLines, mdLines);
                    mdLines = currentFileLines;
                }

                await File.WriteAllLinesAsync(readmeFilePath, mdLines);

                return true;
            }
            catch (Exception e)
            {
                _output.WriteLine($"Failed to create .env file documentation.");
                _output.WriteLine(e.ToString());
                return false;
            }
        }

        private async Task<List<string>> ReadCurrentFileLinesAsync(string readmeFilePath)
        {
            var fileLines = await File.ReadAllLinesAsync(readmeFilePath);
            return fileLines.ToList();
        }

        private void ReplaceLinesInCurrentFile(List<string> currentFileLines, List<string> mdLines)
        {
            var positionInCurrentFile = currentFileLines.IndexOf(mdLines[0]);
            if (positionInCurrentFile > -1)
            {
                do
                {
                    currentFileLines.RemoveAt(positionInCurrentFile);
                }
                while (currentFileLines.Count() > 0
                && positionInCurrentFile < currentFileLines.Count() - 1
                && !currentFileLines[positionInCurrentFile].StartsWith("### "));
            }
            else
            {
                positionInCurrentFile = currentFileLines.Count - 1;
            }

            currentFileLines.InsertRange(positionInCurrentFile, mdLines);
        }

        private void DumpVarNamesByAssembly(List<string> mdLines, List<string> environmentVariables)
        {
            if (string.IsNullOrEmpty(DocumentedProjectName))
            {
                DocumentedProjectName = Assembly.GetExecutingAssembly().GetName().Name;
            }
            mdLines.Add($"### {DocumentedProjectName}");

            environmentVariables
                .ForEach(propertyName =>
                {
                    mdLines.Add($"\t{propertyName}=********");
                });

            mdLines.Add(string.Empty);
        }

        private void InsertHeaderLines(IList<string> lines)
        {
            lines.Insert(0, "## Avoid committing you secrets and keys ");
            lines.Insert(1, "In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following items: ");
            lines.Insert(2, string.Empty);
        }

        private DirectoryInfo GetAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            return new DirectoryInfo(codeBase)!.Parent!;
        }

        private List<string> GetEnvironmentVariableNames()
        {
            var result = new HashSet<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().FullName.StartsWith("Trakx"));

            assemblies
                .ToList()
                .ForEach(currentAssembly =>
                {
                    var assemblyTypes = currentAssembly.GetTypes();

                    assemblyTypes.ToList().ForEach(x =>
                    {
                        var typename = x.FullName;

                        if (string.IsNullOrEmpty(typename)) return;

                        var typeProperties = x.GetProperties();

                        if (typeProperties.Length <= 0) return;

                        foreach (var property in typeProperties)
                        {
                            if (property.GetCustomAttribute(typeof(ReadmeDocumentAttribute)) is ReadmeDocumentAttribute attribute)
                            {
                                result.Add(attribute.VarName);
                            }
                        }
                    });
                });

            return result.ToList();
        }
    }
}