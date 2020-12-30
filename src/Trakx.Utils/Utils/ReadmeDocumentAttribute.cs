using System;

namespace Trakx.Utils.Utils
{
    public class ReadmeDocumentAttribute : Attribute
    {
        public readonly string VarName;

        public ReadmeDocumentAttribute(string varName)
        {
            VarName = varName;
        }
    }
}
