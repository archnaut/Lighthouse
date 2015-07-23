using System.Collections.Generic;

namespace Lighthouse.Common.Interoperability
{
    public class SilverlightAssemblyTestRunSettings
    {
        public string AssemblyName { get; set; }

        private List<string> _methodsToExclude = new List<string>();
        public List<string> MethodsToExclude
        {
            get { return _methodsToExclude; }
            set { _methodsToExclude = value; }
        }

        private List<string> _methodsToInclude = new List<string>();
        public List<string> MethodsToInclude
        {
            get { return _methodsToInclude; }
            set { _methodsToInclude = value; }
        }
    }
}