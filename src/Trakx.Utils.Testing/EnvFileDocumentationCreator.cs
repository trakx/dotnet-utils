using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing
{
    public class EnvFileDocumentationCreator
    {
        private readonly ITestOutputHelper _output;

        public EnvFileDocumentationCreator(ITestOutputHelper output)
        {
            _output = output;
        }

        public async Task<bool> TryCreateEnvFileDocumentation()
        {
            try
            {
                var directory = GetAssemblyDirectory();
                if (!directory.TryWalkBackToRepositoryRoot(out var repositoryRoot))
                    repositoryRoot.Should().NotBeNull("Tests should be running from its repository root.");
                _output.WriteLine($"using {repositoryRoot!.FullName} as repository root path.");
                var readmeFilePath = Path.Combine(repositoryRoot!.FullName, "Environment.md");
                var configProperties = GetConfigurationProperties(directory);

                if (!configProperties.Any()) return true;

                var mdLines = new List<string>();

                AddHeaderLines(mdLines);

                DumpVarNamesByAssembly(mdLines, configProperties);

                AddFullSampleIntro(mdLines);

                BuildSampleEnvFile(mdLines, configProperties);

                if (!File.Exists(readmeFilePath)) File.Delete(readmeFilePath);

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

        private void BuildSampleEnvFile(List<string> mdLines, Dictionary<string, List<string>> configProperties)
        {
            configProperties
                .Values
                .SelectMany(x => x)
                .Distinct()
                .ToList()
                .ForEach(x =>
                {
                    mdLines.Add($"\t{x}");
                });
            mdLines.Add(string.Empty);
        }

        private void DumpVarNamesByAssembly(List<string> mdLines, Dictionary<string, List<string>> configProperties)
        {
            configProperties
                .Keys
                .ToList()
                .ForEach(key =>
                {
                    mdLines.Add($"### {key}");
                    mdLines.Add(string.Empty);

                    configProperties[key]
                    .ToList()
                    .ForEach(propertyName =>
                    {
                        mdLines.Add($"\t{propertyName}=********");
                    });

                });

            mdLines.Add(string.Empty);
        }

        private void AddHeaderLines(IList<string> lines)
        {
            lines.Add("## Avoid committing you secrets and keys ");
            lines.Add("In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following items based on the Configuration classes you instantiate: ");
            lines.Add(string.Empty);
        }

        private void AddFullSampleIntro(IList<string> lines)
        {
            lines.Add("### Complete .env file sample");
            lines.Add(string.Empty);

        }

        private DirectoryInfo GetAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            return new DirectoryInfo(codeBase)!.Parent!;
        }

        private Dictionary<string, List<string>> GetConfigurationProperties(DirectoryInfo dirInfo)
        {
            var result = new Dictionary<string, List<string>>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().FullName.StartsWith("Trakx"));

            assemblies
                .ToList()
                .ForEach(currentAssembly =>
                {
                    var assemblyTypes = currentAssembly.GetTypes();

                    assemblyTypes.ToList().ForEach(x =>
                    {
                        var typename = x.FullName;

                        if (string.IsNullOrEmpty(typename) || !typename.EndsWith("Configuration")) return;
                        var varNames = new List<string>();

                        var typeProperties = x.GetProperties();

                        if (typeProperties.Length <= 0) return;
                        foreach (var property in typeProperties)
                        {
                            if (property.GetCustomAttribute(typeof(ReadmeDocumentAttribute)) is ReadmeDocumentAttribute attribute)
                            {
                                varNames.Add(attribute.VarName);
                            }
                        }

                        if (varNames.Count > 0)
                        {
                            result.Add(typename, varNames);
                        }
                    });
                });

            return result;
        }
    }
}