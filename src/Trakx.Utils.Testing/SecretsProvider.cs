using DotNetEnv;
using System.IO;
using System.Linq;
using System.Reflection;
using Trakx.Utils.Attributes;
using Trakx.Utils.Testing.Interfaces;
using static System.Environment;

namespace Trakx.Utils.Testing
{
    public class SecretsProvider<T> : ISecretsProvider<T> where T : new()
    {
        public SecretsProvider()
        {
            var srcPath = new DirectoryInfo(CurrentDirectory).Parent?.Parent?.Parent?.Parent!;
            Env.Load(Path.Combine(srcPath.FullName, ".env"));
        }

        public T GetSecrets()
        {
            var result = new T();

            var typeProperties = typeof(T)
                .GetProperties()
                .ToList();

            foreach (var property in typeProperties)
            {
                if (property.GetCustomAttribute(typeof(ReadmeDocumentAttribute)) is ReadmeDocumentAttribute attribute)
                {
                    property.SetValue(result, GetEnvironmentVariable(attribute.VarName));
                }
            }

            return result;
        }
    }
}
