using System;
using Lighthouse.Common.Ioc;
using Lighthouse.Common.Services;
using LighthouseDesktop.Core.Infrastructure.FileSystem;
using LighthouseDesktop.Core.Infrastructure.Integration;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;
using LighthouseDesktop.Core.Infrastructure.TestExecution;
using LighthouseDesktop.Core.Infrastructure.TestExecutionEnvironment;
using LighthouseDesktop.Core.Infrastructure.TestResultsConverters;
using LighthouseDesktop.Core.Infrastructure.XapManagement;

namespace LighthouseDesktop.Core
{
    public class Bootstrapper
    {
        public static void Initialize()
        {
            SimpleServiceLocator.Instance.Register<ITestExecutor, TestExecutor>();
            SimpleServiceLocator.Instance.Register<IXapSourcedTestExecutor, XapSourcedTestExecutor>();
            SimpleServiceLocator.Instance.Register<IIndividualDllsSourcesTestExecutor, IndividualDllsSourcesTestExecutor>();

            SimpleServiceLocator.Instance.Register<IXapBuilder, XapBuilder>();
            SimpleServiceLocator.Instance.Register<IXapSourcedLighthouseXapBuilder, XapSourcedLighthouseXapBuilder>();
            SimpleServiceLocator.Instance.Register<ISilverlightDllSourcedLighthouseXapBuilder, SilverlightDllSourcedLighthouseXapBuilder>();


            SimpleServiceLocator.Instance.Register<IHtmlPageBuilder, HtmlPageBuilder>();
            SimpleServiceLocator.Instance.Register<ISimpleResourceManager, SimpleResourceManager>();
            SimpleServiceLocator.Instance.Register<ISilverlightVersionSpecificResourcesProvider, SilverlightVersionSpecificResourcesProvider>();
            SimpleServiceLocator.Instance.Register<IGenericResourcesProvider, GenericResourcesProvider>();
            SimpleServiceLocator.Instance.Register<ISerializationService, SerializationService>();
            SimpleServiceLocator.Instance.Register<IXapReader, XapReader>();
            SimpleServiceLocator.Instance.Register<ITestResultsInformer, TestResultsInformer>();

            SimpleServiceLocator.Instance.Register
                <ISilverlightApplicationManifestAnalyzer, Silverlight4AppManifestAnalyzer>();

            SimpleServiceLocator.Instance.Register
                <ITemplatedSilverlightApplicationManifestGenerator, TemplatedSilverlightApplicationManifestGenerator>();

            SimpleServiceLocator.Instance.Register<INUnitXmlResultsFileCreator, NUnitXmlResultsFileCreator>();

            SimpleServiceLocator.Instance.Register<IMultiLogger, MultiLogger>(new SingletonCreationFactory());
            SimpleServiceLocator.Instance.Register<IConsoleLogger, ConsoleLogger>();
            SimpleServiceLocator.Instance.Register<IFileLogger, FileLogger>();

            SimpleServiceLocator.Instance.RegisterInstance<ILogger>(SimpleServiceLocator.Instance.Get<IMultiLogger>());

            SimpleServiceLocator.Instance.Register<ITestExecutionOrchectrator, TestExecutionOrchectrator>();

            SimpleServiceLocator.Instance.Register<ICommandLineArgsParser, ParameteroCommandLineArgsParserAdapter>();
            SimpleServiceLocator.Instance.Register<ICleanupManager,CleanupManager>(new SingletonCreationFactory());

            SimpleServiceLocator.Instance.Register<IWildcardPathsParser, WildcardPathsParser>();
        }
    }
}