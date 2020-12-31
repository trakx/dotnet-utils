using System;

namespace Trakx.Utils.Attributes
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
