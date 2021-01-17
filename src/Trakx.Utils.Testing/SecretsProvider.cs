using DotNetEnv;
using System.IO;
using System.Linq;
using System.Reflection;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Trakx.Utils.Testing.Interfaces;
using static System.Environment;

namespace Trakx.Utils.Testing
{
    public class SecretsProvider<T> : ISecretsProvider<T> where T : new()
    {
        public SecretsProvider()
        {
            var envFilePath = DirectoryInfoExtensions.GetDefaultEnvFilePath(null);
            if (envFilePath != null) Env.Load(Path.Combine(envFilePath));
        }

        public T GetSecrets()
        {
            var result = new T();

            var typeProperties = typeof(T)
                .GetProperties()
                .ToList();

            foreach (var property in typeProperties)
            {
                if (property.GetCustomAttribute(typeof(SecretEnvironmentVariableAttribute)) is SecretEnvironmentVariableAttribute attribute)
                {
                    property.SetValue(result, GetEnvironmentVariable(attribute.VarName));
                }
            }

            return result;
        }
    }
}
