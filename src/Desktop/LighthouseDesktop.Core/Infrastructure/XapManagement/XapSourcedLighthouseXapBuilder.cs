using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface IXapSourcedLighthouseXapBuilder
    {
        XapSourcedXapBuildResult CreateXapFromXap(XapSourcedXapBuildParameters parameters);
    }

    public class XapSourcedXapBuildParameters
    {
        public string SourceXapPath { get; set; }
        public string OutputXapPath { get; set; }
    }

    public class XapSourcedLighthouseXapBuilder : IXapSourcedLighthouseXapBuilder
    {
        private IXapBuilder _outputXapFileBuilder;
        private ISilverlightVersionSpecificResourcesProvider _silverlightVersionSpecificResourcesProvider;
        private IGenericResourcesProvider _genericResourcesProvider;
        private readonly ITemplatedSilverlightApplicationManifestGenerator _manifestGenerator;
        private readonly ISilverlightApplicationManifestAnalyzer _manifestAnalyzer;
        private readonly IXapReader _xapReader;

        public XapSourcedLighthouseXapBuilder(
            IXapBuilder outputXapFileBuilder, 
            ISilverlightVersionSpecificResourcesProvider resourceManager, 
            IGenericResourcesProvider simpleResourceManager, 
            ITemplatedSilverlightApplicationManifestGenerator manifestGenerator, 
            ISilverlightApplicationManifestAnalyzer manifestAnalyzer,
            IXapReader xapReader
            )
        {
            _outputXapFileBuilder = outputXapFileBuilder;
            _silverlightVersionSpecificResourcesProvider = resourceManager;
            _genericResourcesProvider = simpleResourceManager;
            _manifestGenerator = manifestGenerator;
            _manifestAnalyzer = manifestAnalyzer;
            _xapReader = xapReader;

            SilverlightVersion = 4;
        }

        public int SilverlightVersion { get; set; }

        public XapSourcedXapBuildResult CreateXapFromXap(XapSourcedXapBuildParameters inputParameters)
        {
            var xapBuildResult = new XapSourcedXapBuildResult() {ResultingXapFullPath = inputParameters.OutputXapPath, SourceXapFullPath = inputParameters.SourceXapPath};

            var fi = new FileInfo(inputParameters.SourceXapPath);

            if (!fi.Exists)
            {
                throw new Exception(string.Format("Source XAP {0} does not exist.", inputParameters.SourceXapPath));                
            }

/*            if (ZipFile.IsZipFile(sourceXapPath))
            {
                throw new Exception(string.Format("Source XAP {0} is not valid zip file.", sourceXapPath));                                
            }*/

            _outputXapFileBuilder.FullXapPath = inputParameters.OutputXapPath;

            InitializeOutputXapBuilderAndAddNecessaryLighthouseDlls();

            _xapReader.Load(inputParameters.SourceXapPath);

            var sourceManifest = _xapReader.GetManifest();
            if (string.IsNullOrEmpty(sourceManifest))
            {
                throw new Exception("Source xap does not have an AppManifest.xaml");
            }

            SilverlightManifestAnalysisResult sourceManifestInfo = _manifestAnalyzer.Analyze(sourceManifest);
            xapBuildResult.SourceXapAnalysisResult = sourceManifestInfo;

            var lighthouseDllNames = new Collection<string>()
                                         {
                                             Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingDllName,
                                             Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingQualityToolsDllName,
                                             Constants.ResourceNames.DllNames.LighthouseSilverlightTestRunnerAppDllName,
                                             Constants.ResourceNames.DllNames.LighthouseSilverlightCoreDllName,
                                             Constants.ResourceNames.DllNames.LighthouseSilverlightCommonDllName,
                                         };

            _manifestGenerator.ManifestTemplate = _genericResourcesProvider.GetResourceContent("AppManifestTemplate.xaml");
            _manifestGenerator.AdditionalAssembyPartsInjectionPlaceholder = "{ADDITIONAL_ASSEMBLY_PARTS}";

            foreach (var manifestAssemblyPartItem in sourceManifestInfo.AssemblyPartItems)
            {
                if (!lighthouseDllNames.Contains(manifestAssemblyPartItem.Source))
                {
                   _manifestGenerator.AddAssemblyPartItem(manifestAssemblyPartItem);
                }
            }

            var generatedApplicationManifest = _manifestGenerator.GenerateNewApplicationManifest();
            _outputXapFileBuilder.AddFileToXap("AppManifest.xaml", generatedApplicationManifest);
           
            foreach (var xapFileName in _xapReader.Files)
            {
                if (xapFileName != "AppManifest.xaml" && !lighthouseDllNames.Contains(xapFileName))
                {
                    _outputXapFileBuilder.AddFileToXap(xapFileName, _xapReader.GetFileBytes(xapFileName));
                }
            }

            _outputXapFileBuilder.Save();

            xapBuildResult.Success = true;

            return xapBuildResult;
        }

        protected void InitializeOutputXapBuilderAndAddNecessaryLighthouseDlls()
        {
            _outputXapFileBuilder.Clear();

            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingDllName));
            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingQualityToolsDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.DllNames.MicrosoftSilverlightTestingQualityToolsDllName));

            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.DllNames.LighthouseSilverlightTestRunnerAppDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.DllNames.LighthouseSilverlightTestRunnerAppDllName));
            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.PdbNames.LighthouseSilverlightTestRunnerAppDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.PdbNames.LighthouseSilverlightTestRunnerAppDllName));

            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.DllNames.LighthouseSilverlightCoreDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.DllNames.LighthouseSilverlightCoreDllName));
            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.PdbNames.LighthouseSilverlightCoreDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.PdbNames.LighthouseSilverlightCoreDllName));

            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.DllNames.LighthouseSilverlightCommonDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.DllNames.LighthouseSilverlightCommonDllName));
            _outputXapFileBuilder.AddFileToXapIfNotAlreadyThere(Constants.ResourceNames.PdbNames.LighthouseSilverlightCommonDllName, _silverlightVersionSpecificResourcesProvider.GetResourceStream(Constants.ResourceNames.PdbNames.LighthouseSilverlightCommonDllName));
        }

    }
}