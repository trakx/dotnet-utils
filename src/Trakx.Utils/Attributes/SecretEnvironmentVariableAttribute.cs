using System;

namespace Trakx.Utils.Attributes
{
    public class SecretEnvironmentVariableAttribute : Attribute
    {
        public string? VarName { get; }

        public SecretEnvironmentVariableAttribute(string? varName = null)
        {
            VarName = varName;
        }
    }
}
