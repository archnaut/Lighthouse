using System.Collections.Generic;
using System.IO;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface ISilverlightApplicationManifestAnalyzer
    {
        SilverlightManifestAnalysisResult Analyze(string manifestContent);
        SilverlightManifestAnalysisResult Analyze(Stream manifestStream);
    }
}