using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Xunit;

namespace Trakx.Utils.Testing.Tests.Integration
{
    public class ReadmeEditorTests
    {
        [Fact]
        public async Task ReadmeEditor_should_not_allow_concurrent_edits()
        {
            var file = Path.GetTempFileName();
            await using var writer = new StreamWriter(file);
            await writer.WriteAsync("hello ");
            await writer.DisposeAsync();

            var edits = Enumerable.Repeat("*", 10).Select(async s =>
            {
                using var editor = new ReadmeEditor(file);
                var content = await editor.ExtractReadmeContent();
                await editor.UpdateReadmeContent(content + s);
            }).ToArray();

            await Task.WhenAll(edits);

            var delays = Enumerable.Repeat(TimeSpan.FromMilliseconds(100), 100);
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(delays);
            var trials = await retryPolicy.ExecuteAndCaptureAsync(async () => await File.ReadAllTextAsync(file));
            trials.Result.Should().Be("hello **********");
        }
    }
}