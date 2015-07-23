using System.Collections.Generic;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public class XapBuildResult : IXapBuildResult
    {
        public bool Success { get; set; }

        private IList<string> _errors = new List<string>();
        public IList<string> Errors
        {
            get { return _errors; }
            private set { _errors = value; }
        }

        public string ResultingXapFullPath { get; set; }        
    }
}