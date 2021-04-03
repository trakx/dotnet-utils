using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Trakx.Utils.Testing
{
    internal interface IReadmeEditor
    {
        Task<string> ExtractReadmeContent();
        Task UpdateReadmeContent(string newContent);
    }
}