using System;
using System.Collections.Generic;

namespace Lighthouse.Common.Interoperability
{
    public class SilverlightUnitTestRunSettings
    {
        private List<string> _ssembliesThatContainTests = new List<string>();
        public List<string> AssembliesThatContainTests
        {
            get { return _ssembliesThatContainTests; }
            set { _ssembliesThatContainTests = value; }
        }

        public string TagFilter { get; set; }
    }
}