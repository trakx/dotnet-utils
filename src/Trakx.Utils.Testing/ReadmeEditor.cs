using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace Trakx.Utils.Testing
{
    internal sealed class ReadmeEditor : IReadmeEditor, IDisposable
    {
        private readonly AsyncRetryPolicy _retryPolicy;
        private FileStream? _stream;
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore;

        public ReadmeEditor(string filePath)
        {
            _filePath = filePath;
            var delays = Enumerable.Repeat(TimeSpan.FromMilliseconds(100), 100);
            _retryPolicy = Policy.Handle<IOException>().WaitAndRetryAsync(delays);
            _semaphore = new SemaphoreSlim(1, 1);
        }
        
        public async Task<string> ExtractReadmeContent()
        {
            await _semaphore.WaitAsync();
            _stream ??= await GetExclusiveFileStream();
            _stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_stream, leaveOpen: true);
            var result = await reader.ReadToEndAsync();
            return result;
        }

        private async Task<FileStream> GetExclusiveFileStream()
        {
            var getStream = await _retryPolicy
                .ExecuteAndCaptureAsync(() => Task.FromResult(new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)));
            var stream = getStream.Result;
            return stream;
        }

        public async Task UpdateReadmeContent(string newContent)
        {
            _stream ??= await GetExclusiveFileStream();
            _stream.SetLength(0);
            _stream.Seek(0, SeekOrigin.Begin);

            await using var writer = new StreamWriter(_stream, leaveOpen: true);
            await writer.WriteAsync(newContent);
            await writer.FlushAsync();
            _semaphore.Release(1);
        }
         
        public void Dispose()
        {
            _stream?.FlushAsync().GetAwaiter().GetResult();
            _stream?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}