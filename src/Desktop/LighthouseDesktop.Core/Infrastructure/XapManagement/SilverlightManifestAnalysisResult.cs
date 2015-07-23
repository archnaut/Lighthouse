using System.Collections.Generic;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public class SilverlightManifestAnalysisResult
    {
        private List<ManifestAssemblyPartItem> _assemblyPartItems = new List<ManifestAssemblyPartItem>();
        public List<ManifestAssemblyPartItem> AssemblyPartItems
        {
            get { return _assemblyPartItems; }
            set { _assemblyPartItems = value; }
        }

        public string EntryPointAssemblyName { get; set; }
        public string EntryPointAssemblyDllName { get; set; }
        public string EntryPointTypeName { get; set; }

    }
}