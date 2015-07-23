using System.Collections.Generic;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface IXapBuildResult
    {
        bool Success { get; }
        IList<string> Errors { get; }
        string ResultingXapFullPath { get; set;  }
    }

    public class XapSourcedXapBuildResult : XapBuildResult
    {
        public SilverlightManifestAnalysisResult SourceXapAnalysisResult { get; set; }
        public string SourceXapFullPath { get; set; }
    }
}