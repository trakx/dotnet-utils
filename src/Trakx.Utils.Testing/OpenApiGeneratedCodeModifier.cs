﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trakx.Utils.Testing.Attributes;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable S2699

namespace Trakx.Utils.Testing
{
    [TestCaseOrderer(RunOrderAttributeOrderer.TypeName, RunOrderAttributeOrderer.AssemblyName)]
    public abstract class OpenApiGeneratedCodeModifier
    {
        private readonly ITestOutputHelper _output;
        protected List<string> FilePaths { get; } = new ();

        private static readonly string ClassDefinitionRegex = @"(?<before>[\s]{4}\[System\.CodeDom\.Compiler\.GeneratedCode\(""NJsonSchema"", [^\]]+\]\r?\n[\s]{4}public partial )(?<class>class)(?<after> [\w:\s]+\s?\r?\n)";
        private static readonly string SetPropertyRegex = @"(?<before>[\s]{8}\[Newtonsoft\.Json\.JsonProperty\([^\]]+\]\r?\n[\s]{8}public [^\s]+ [^\s]+ \{ get; )(?<set>set)";
        private static readonly string ClientRegex = @"public partial interface I(?<client>[^\s]+)";

        protected OpenApiGeneratedCodeModifier(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact, RunOrder(1)]
        public async Task Replace_model_class_by_records()
        {
            foreach (var filePath in FilePaths)
            {
                var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                content = Regex.Replace(content, ClassDefinitionRegex, "${before}" + "record" + "${after}");
                content = Regex.Replace(content, SetPropertyRegex, "${before}" + "init");
                await File.WriteAllTextAsync(filePath, content);
            }
        }

        [Fact, RunOrder(2)]
        public async Task Remove_warnings()
        {
            foreach (var filePath in FilePaths)
            {
                var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                var existingWarningStart = @"#pragma warning disable 108";
                var warningStartToAdd = @"#pragma warning disable CS0618";
                if (!content.Contains(warningStartToAdd)) content = content.Replace(existingWarningStart,
                     warningStartToAdd + Environment.NewLine + existingWarningStart);
                var existingWarningStop = @"#pragma warning restore 108";
                var warningStopToAdd = @"#pragma warning restore CS0618";
                if (!content.Contains(warningStopToAdd)) content = content.Replace(existingWarningStop,
                    existingWarningStop + Environment.NewLine + warningStopToAdd);
                await File.WriteAllTextAsync(filePath, content); 
            }
        }


        [Fact, RunOrder(3)]
        public async Task Output_client_names()
        {
            foreach (var filePath in FilePaths)
            {
                var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                var clientRegex = new Regex(ClientRegex);
                var clients = clientRegex.Matches(content).Select(m => m.Groups["client"].Value)
                    .OrderBy(s => s);
                _output.WriteLine(string.Join(", ", clients.Select(c => $"\"{c}\""))); 
            }
        }

        
        [Fact, RunOrder(4)]
        public async Task Do_not_call_send_methods_asynchronously()
        {
            foreach (var filePath in FilePaths)
            {
                var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                content = content.Replace(@"var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);",
                @"var response_ = client_.Send(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);");
                await File.WriteAllTextAsync(filePath, content);
            }
        }
    }
}

#pragma warning restore S2699