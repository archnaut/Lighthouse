using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Services;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;
using LighthouseDesktop.Core.Infrastructure.TestExecutionEnvironment;
using LighthouseDesktop.Core.Infrastructure.XapManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public interface IXapSourcedTestExecutor : ITestExecutor
    {
        string SourceXapFullPath { get; set; }
    }

    public class XapSourcedTestExecutor : TestExecutor, IXapSourcedTestExecutor
    {
        private readonly IXapSourcedLighthouseXapBuilder _xapBuilder;

        public XapSourcedTestExecutor(IHtmlPageBuilder htmlPageBuilder, ISerializationService serializationService, IXapSourcedLighthouseXapBuilder xapBuilder, ITestResultsInformer testResultsInformer, ILogger logger, ICleanupManager cleanupManager)
            : base(htmlPageBuilder, serializationService, testResultsInformer, logger, cleanupManager)
        {
            _xapBuilder = xapBuilder;
        }

        public override RemoteTestExecutionResults Execute()
        {
            var xapBuildResult =
                _xapBuilder.CreateXapFromXap(new XapSourcedXapBuildParameters()
                                                 {SourceXapPath = SourceXapFullPath, OutputXapPath = OutputXapFullPath});

            TestExecutionResults.XapBuildResult = xapBuildResult;

            if (!xapBuildResult.Success)
            {
                TestExecutionResults.RunWasComplete = false;
                TestExecutionResults.ExecutionErrors.Add("Error while creating XAP file.");
                return TestExecutionResults;
            }

            var xapUri = new Uri(xapBuildResult.ResultingXapFullPath, UriKind.Absolute);

            TestExecutionSettings.XapUri = xapUri;

            if (!string.IsNullOrEmpty(xapBuildResult.SourceXapAnalysisResult.EntryPointAssemblyDllName))
            {
                TestExecutionSettings.SilverlightUnitTestRunSettings.AssembliesThatContainTests.Add(xapBuildResult.SourceXapAnalysisResult.EntryPointAssemblyDllName);
            }

            return base.Execute();
        }

        public string SourceXapFullPath { get; set; }
    }
}