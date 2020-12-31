using System;

namespace Trakx.Utils.Utils
{
    public class ReadmeDocumentAttribute : Attribute
    {
        private readonly string _varName;
        public string VarName => _varName;
        public ReadmeDocumentAttribute(string varName)
        {
            _varName = varName;
        }
    }
}
