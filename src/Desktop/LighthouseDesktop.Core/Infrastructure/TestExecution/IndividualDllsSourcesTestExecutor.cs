using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Services;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;
using LighthouseDesktop.Core.Infrastructure.TestExecutionEnvironment;
using LighthouseDesktop.Core.Infrastructure.XapManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public interface IIndividualDllsSourcesTestExecutor : ITestExecutor
    {
        IList<string> AssembliesWithTests { get; set; }
        IList<string> ReferencedFiles { get; set; }
    }
    
    public class IndividualDllsSourcesTestExecutor : TestExecutor, IIndividualDllsSourcesTestExecutor
    {
        private readonly ISilverlightDllSourcedLighthouseXapBuilder _xapBuilder;

        public IndividualDllsSourcesTestExecutor(IHtmlPageBuilder htmlPageBuilder, ISerializationService serializationService, ISilverlightDllSourcedLighthouseXapBuilder xapBuilder, ITestResultsInformer testResultsInformer, ILogger logger, ICleanupManager cleanupManager) : base(htmlPageBuilder, serializationService, testResultsInformer, logger, cleanupManager)
        {
            _xapBuilder = xapBuilder;
        }

        private IList<string> _assembliesWithTests = new List<string>();
        public IList<string> AssembliesWithTests
        {
            get { return _assembliesWithTests; }
            set { _assembliesWithTests = value; }
        }

        private IList<string> _referencedAssemblies = new List<string>();
        public IList<string> ReferencedFiles
        {
            get { return _referencedAssemblies; }
            set { _referencedAssemblies = value; }
        }

        public override RemoteTestExecutionResults Execute()
        {
            var xapBuildResult = _xapBuilder.CreateXap(new SilverlightDllSourcedXapBuilderParameters()
                                                           {
                                                               OutputXapPath = OutputXapFullPath, 
                                                               FilesForXap =  AssembliesWithTests.Union(ReferencedFiles).ToList()
                                                           });

            TestExecutionResults.XapBuildResult = xapBuildResult;

            if (!xapBuildResult.Success)
            {
                TestExecutionResults.RunWasComplete = false;
                TestExecutionResults.ExecutionErrors.Add("Error while creating XAP file.");
                return TestExecutionResults;
            }

            var xapUri = new Uri(xapBuildResult.ResultingXapFullPath, UriKind.Absolute);

            TestExecutionSettings.XapUri = xapUri;

            var silverlightTestRunSettings = new SilverlightUnitTestRunSettings();

            foreach (var sourceDll in AssembliesWithTests)
            {
                silverlightTestRunSettings.AssembliesThatContainTests.Add(Path.GetFileName(sourceDll));                
            }

            TestExecutionSettings.SilverlightUnitTestRunSettings = silverlightTestRunSettings;

            return base.Execute();
        }
    }
}