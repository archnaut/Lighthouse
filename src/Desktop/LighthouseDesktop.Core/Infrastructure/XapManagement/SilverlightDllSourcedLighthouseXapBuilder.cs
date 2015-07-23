using System.Collections.Generic;
using System.IO;
using System.Linq;
using LighthouseDesktop.Core.Infrastructure.FileSystem;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{

    public class SilverlightDllSourcedXapBuilderParameters
    {
        private IList<string> _filesForXap = new List<string>();
        public IList<string> FilesForXap
        {
            get { return _filesForXap; }
            set { _filesForXap = value; }
        }

        public string OutputXapPath { get; set; }
    }

    public interface ISilverlightDllSourcedLighthouseXapBuilder
    {
        XapBuildResult CreateXap(SilverlightDllSourcedXapBuilderParameters parameters);
    }

    public class SilverlightDllSourcedLighthouseXapBuilder : ISilverlightDllSourcedLighthouseXapBuilder
    {
        private readonly IXapBuilder _outputXapFileBuilder;
        private readonly ITemplatedSilverlightApplicationManifestGenerator _manifestGenerator;
        private readonly ILogger _logger;
        private readonly IWildcardPathsParser _wildcardPathsParser;
        private readonly ISilverlightVersionSpecificResourcesProvider _silverlightVersionSpecificResourcesProvider;
        private readonly IGenericResourcesProvider _genericResourcesProvider;

        public SilverlightDllSourcedLighthouseXapBuilder(IXapBuilder outputXapFileBuilder,
            ISilverlightVersionSpecificResourcesProvider silverlightVersionSpecificResourcesProvider,
            IGenericResourcesProvider genericResourcesProvider,
            ITemplatedSilverlightApplicationManifestGenerator manifestGenerator,
            ILogger logger,
            IWildcardPathsParser wildcardPathsParser)
        {
            _outputXapFileBuilder = outputXapFileBuilder;
            _silverlightVersionSpecificResourcesProvider = silverlightVersionSpecificResourcesProvider;
            _genericResourcesProvider = genericResourcesProvider;
            _manifestGenerator = manifestGenerator;
            _logger = logger;
            _wildcardPathsParser = wildcardPathsParser;

            SilverlightVersion = 4;
        }

        public int SilverlightVersion
        {
            get { return _silverlightVersionSpecificResourcesProvider.SilverlightVersion; }
            set
            {
                _silverlightVersionSpecificResourcesProvider.SilverlightVersion = value;
            }
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

        public XapBuildResult CreateXap(SilverlightDllSourcedXapBuilderParameters parameters)
        {
            var result = new XapBuildResult() {ResultingXapFullPath = parameters.OutputXapPath};

            if (parameters.FilesForXap == null || !parameters.FilesForXap.Any())
            {
                return result;
            }

            _outputXapFileBuilder.FullXapPath = parameters.OutputXapPath;

            InitializeOutputXapBuilderAndAddNecessaryLighthouseDlls();

            _manifestGenerator.ManifestTemplate = _genericResourcesProvider.GetResourceContent("AppManifestTemplate.xaml");
            _manifestGenerator.AdditionalAssembyPartsInjectionPlaceholder = "{ADDITIONAL_ASSEMBLY_PARTS}";

            try
            {
                foreach (var path in parameters.FilesForXap)
                {
                    AddReferencedFileBasedonItsType(path);
                }
            }
            catch (FileNotFoundException e)
            {
                _logger.Log(e.Message);
                result.Errors.Add(e.Message);
                return result;
            }

            var generatedApplicationManifest = _manifestGenerator.GenerateNewApplicationManifest();
            _outputXapFileBuilder.AddFileToXap("AppManifest.xaml", generatedApplicationManifest);

            _outputXapFileBuilder.Save();

            result.Success = true;

            return result;
        }

        private void AddReferencedFileBasedonItsType(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
            {
                throw new FileNotFoundException(string.Format("Could not find referenced file {0}.", fileFullPath), fileFullPath);
            }

            var fileName = Path.GetFileName(fileFullPath);

            if (_outputXapFileBuilder.FileIsAlreadyInXap(fileName))
            {
                _logger.Log(string.Format("Skipping file {0} since its already in the XAP.", fileName));
                return;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileFullPath);
            _outputXapFileBuilder.AddFileToXap(Path.GetFileName(fileFullPath), File.OpenRead(fileFullPath));

            if (fileFullPath.ToLower().EndsWith(".dll"))
            {
                _manifestGenerator.AddAssemblyPartItem(new ManifestAssemblyPartItem()
                                                           {
                                                               Name = fileNameWithoutExtension,
                                                               Source = fileName
                                                           });
            }
        }
    }
}