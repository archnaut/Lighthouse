using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Ioc;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using LighthouseDesktop.Core.Infrastructure.FileSystem;
using LighthouseDesktop.Core.Infrastructure.Integration;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public interface ITestExecutionOrchectrator
    {
        void Execute();
        void Cleanup();
        void Abort();
    }

    public class TestExecutionOrchectrator : ITestExecutionOrchectrator
    {
        private readonly IMultiLogger _logger;
        private readonly IFileLogger _fileLogger;
        private readonly ICommandLineArgsParser _cmdArgsParser;
        private readonly ICleanupManager _cleanupManager;
        private readonly IWildcardPathsParser _wildcardPathsParser;
        private bool _skipCleanup;
        private ITestExecutor _currentTestExecutor;

        public TestExecutionOrchectrator(IMultiLogger multiLogger, IFileLogger fileLogger, ICommandLineArgsParser cmdArgsParser, ICleanupManager cleanupManager, IWildcardPathsParser wildcardPathsParser)
        {
            _logger = multiLogger;
            _fileLogger = fileLogger;
            fileLogger.IsActive = false;
            multiLogger.AddLogger(fileLogger);
            _cmdArgsParser = cmdArgsParser;
            _cleanupManager = cleanupManager;
            _wildcardPathsParser = wildcardPathsParser;
        }

        private static void ShowConsoleHelp()
        {
            Console.WriteLine(string.Format("Lighthouse v{0} (c) 2011 Slobodan Pavkov http://lighthouse.codeplex.org", Constants.Versions.ConsoleRunnerVersionText));
            Console.WriteLine("");
            Console.WriteLine("Parameters:");
            Console.WriteLine("-h (-help)\t Shows this help");
            Console.WriteLine("-m (-mode)\t Operation Mode");
            Console.WriteLine("\t\t -m:Xap - tests the given Silverlight Xap file");
            Console.WriteLine("\t\t -m:Assembly - tests the given list of Silverlight assemblies");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"-m:Xap d:\Xaps\SomeApp.xap d:\tmp\Results.xml");
            Console.WriteLine(@"-m:Dll -tests:""d:\tmp\Assembly1.dll"",""d:\tmp\Assembly2.dll"" -references:""d:\tmp\Needed1.dll"",""d:\tmp\Needed2.dll"" d:\tmp\Results.xml");
            Console.WriteLine("");
            Console.WriteLine("-cnp (-cleanup)\t Cleanup mode");
            Console.WriteLine("Examples:");
            Console.WriteLine("-cnp:On Cleans up all temp files after completed.");
            Console.WriteLine("-cnp:Off Does not clean any temp files after completed.");
            Console.WriteLine("");
            Console.WriteLine("-tf (-TagFilter)\t Tag Filter string - Specifies to run only tests marked with certain Tag(s)  ");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"-tf:""Logging""");
            Console.WriteLine("");
            Console.WriteLine("-lf (-LogFile)\t Log file");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"-lf:""d:\temp\log.txt""");
            Console.WriteLine("");
            Console.WriteLine("-wd (-WorkDir)\t Working Directory");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"-wd:""d:\tmp\LighthouseWorkDir""");
            Console.WriteLine("");
            Console.WriteLine("-tm (-TimeOut)\t Sliding Timeout in Seconds");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"-tm:30");
        }

        private void FillTagFilterIfNeeded()
        {
            if (_cmdArgsParser == null || _currentTestExecutor == null)
            {
                return;
            }

            if (_cmdArgsParser.ParameterIsSet("tf"))
            {
                var value = _cmdArgsParser.GetParameterValue("tf");
                if (!string.IsNullOrEmpty(value))
                {
                    _currentTestExecutor.TestExecutionSettings.SilverlightUnitTestRunSettings.TagFilter = value;
                    _logger.Log(string.Format("Setting Filter Tag: {0}", value));
                }
            }
        }

        private void AssignCommonSettingsThatAreAvailableInCommandLineArgs()
        {
            SetupCleanupConfigurationFromCommandLineArgs();
            FillTagFilterIfNeeded();
            SetupWorkDir();
            SetupResultsOutputFile();
        }


        private bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private void SetupWorkDir()
        {
            if (_cmdArgsParser == null || _currentTestExecutor == null)
            {
                return;
            }

            if (_cmdArgsParser.ParameterIsSet("wd"))
            {
                var value = _cmdArgsParser.GetParameterValue("wd");
                if (!string.IsNullOrEmpty(value))
                {
                    if (!Directory.Exists(value))
                    {
                        Directory.CreateDirectory(value);
                    }

                    if (Directory.Exists(value) && hasWriteAccessToFolder(value))
                    {
                        var fullPath = Path.GetFullPath(value);
                        _currentTestExecutor.WorkDirectory = fullPath;
                        _logger.Log(string.Format("Setting Work Directory to : {0}", fullPath));
                    }
                    else
                    {
                        _logger.Log(string.Format("Could not set Work Directory to : {0}", value));
                    }
                }
            }           
        }

        private void SetupResultsOutputFile()
        {
            if (_currentTestExecutor is IXapSourcedTestExecutor)
            {
                if (_cmdArgsParser.Arguments.Count > 1)
                {
                    _currentTestExecutor.TestResultsOutputFileFullPath = _cmdArgsParser.Arguments[1];
                    _logger.Log(string.Format("Test Results file name: {0}",
                                              _currentTestExecutor.TestResultsOutputFileFullPath));
                }
            }
            else if (_currentTestExecutor is IIndividualDllsSourcesTestExecutor)
            {
                if (_cmdArgsParser.Arguments.Count > 0)
                {
                    _currentTestExecutor.TestResultsOutputFileFullPath = _cmdArgsParser.Arguments[0];
                    _logger.Log(string.Format("Test Results file name: {0}",
                                              _currentTestExecutor.TestResultsOutputFileFullPath));
                }                
            }
        }

        public void Execute()
        {
            SetupCommandLineArgsSynonyms();

            _cmdArgsParser.Parse(Environment.GetCommandLineArgs().Skip(1).ToArray());

            if (!_cmdArgsParser.Arguments.Any() || _cmdArgsParser.ParameterIsSet("?"))
            {
                ShowConsoleHelp();
                return;
            }

            SetupFileLogger();

            if (!_cmdArgsParser.ParameterIsSet("m") || (_cmdArgsParser.ParameterIsSet("m") && _cmdArgsParser.GetParameterValue("m").ToLower() == "xap"))
            {
                // xap mode
                if (!_cmdArgsParser.Arguments.Any())
                {
                    _logger.Log("Please supply Xap file to test.");
                    ShowConsoleHelp();
                    return;                    
                }

                var xapPath = _cmdArgsParser.Arguments.FirstOrDefault();

                var xapSourcedTestExecutor = SimpleServiceLocator.Instance.Get<IXapSourcedTestExecutor>();
                _currentTestExecutor = xapSourcedTestExecutor;
                xapSourcedTestExecutor.SourceXapFullPath = xapPath;
            }
            else if (_cmdArgsParser.ParameterIsSet("m") && _cmdArgsParser.GetParameterValue("m").ToLower() == "dll")
            {
                // assemblies mode

                if (!_cmdArgsParser.ParameterIsSet("tests"))
                {
                    _logger.Log("Please name the full paths of assemblies to test via -tests parameter.");
                    ShowConsoleHelp();
                    return;                     
                }

                IList<string> realTestDllPaths;

                var testsDllPaths = GetListOfAssembliesFromParameter(_cmdArgsParser.GetParameterValue("tests"));
                if (!testsDllPaths.Any())
                {
                    _logger.Log("Parameter -tests must name full paths to all assemblies with tests.");
                    ShowConsoleHelp();
                    return;                      
                }
                else
                {
                    try
                    {
                        realTestDllPaths = _wildcardPathsParser.ConvertPathsWithWildcardsToIndividualPaths(testsDllPaths);
                    }
                    catch (FileNotFoundException e)
                    {
                        _logger.Log(string.Format("Assembly {0} in list of assemblies with tests could not be found", e.FileName));
                        return;  
                    }                    
                }

                IList<string> realReferencedFilesPaths = new List<string>(); 
                IList<string> referencedDlls = new List<string>();
                if (_cmdArgsParser.ParameterIsSet("references"))
                {
                    referencedDlls = GetListOfAssembliesFromParameter(_cmdArgsParser.GetParameterValue("references"));
                }

                if (referencedDlls.Any())
                {
                    try
                    {
                        realReferencedFilesPaths = _wildcardPathsParser.ConvertPathsWithWildcardsToIndividualPaths(referencedDlls);
                    }
                    catch (FileNotFoundException e)
                    {
                        _logger.Log(string.Format("File {0} in list of references could not be found", e.FileName));
                        return;                          
                    }
                }
              
                var individualDllsSourcesTestExecutor = SimpleServiceLocator.Instance.Get<IIndividualDllsSourcesTestExecutor>();
                _currentTestExecutor = individualDllsSourcesTestExecutor;

                individualDllsSourcesTestExecutor.AssembliesWithTests = realTestDllPaths;
                individualDllsSourcesTestExecutor.ReferencedFiles = realReferencedFilesPaths;
            }

            if (!SetupTimeoutFromParamsIfAvailable())
            {
                _logger.Log("Invalid Timeout parameter value supplied.");
                ShowConsoleHelp();
                return;
            }

            AssignCommonSettingsThatAreAvailableInCommandLineArgs();

            _currentTestExecutor.Started += OnStarted;

            _logger.Log("Sending signal to Lighthouse Test Executor to start executing tests.");

            try
            {
                var results = _currentTestExecutor.Execute();
                OnFinished(results);
            }
            catch (Exception e)
            {
                _logger.Log(string.Format("Error occured: {0}", e.Message));
            }


            _logger.Log("Testing process completed.");           
        }

        private void SetupCleanupConfigurationFromCommandLineArgs()
        {
            if (_cmdArgsParser.ParameterIsSet("cnp") && _cmdArgsParser.GetParameterValue("cnp").ToLower() == "off")
            {
                _skipCleanup = true;
            }
        }

        private void SetupCommandLineArgsSynonyms()
        {
            _cmdArgsParser.AddParameterSynonym("Help", new List<string>()
                                                           {
                                                               {"?"},
                                                               {"h"},
                                                               {"hlp"},
                                                               {"help"}
                                                           });

            _cmdArgsParser.AddParameterSynonym("Mode", new List<string>()
                                                           {
                                                               {"mode"},
                                                               {"m"}
                                                           });

            _cmdArgsParser.AddParameterSynonym("Cleanup", new List<string>()
                                                              {
                                                                  {"cleanup"},
                                                                  {"cleanupmode"},
                                                                  {"cnp"}
                                                              });

            _cmdArgsParser.AddParameterSynonym("Log", new List<string>()
                                                          {
                                                              {"lf"},
                                                              {"logfile"},
                                                              {"log"},
                                                          });

            _cmdArgsParser.AddParameterSynonym("Time Out", new List<string>()
                                                               {
                                                                   {"tm"},
                                                                   {"timeout"},
                                                               });

            _cmdArgsParser.AddParameterSynonym("Tag Filter", new List<string>()
                                                                 {
                                                                     {"tf"},
                                                                     {"tagfilter"},
                                                                 });

            _cmdArgsParser.AddParameterSynonym("Work Dir", new List<string>()
                                                                 {
                                                                     {"wd"},
                                                                     {"workdir"},
                                                                 });
        }

        private bool SetupTimeoutFromParamsIfAvailable()
        {
            if (_cmdArgsParser.ParameterIsSet("timeout"))
            {
                var value = _cmdArgsParser.GetParameterValue("timeout");
                int timeout;
                if (!int.TryParse(value, out timeout))
                {
                    return false;
                }

                _currentTestExecutor.TestExecutionSettings.TimeoutInSeconds = timeout;
            }

            return true;
        }

        private void SetupFileLogger()
        {
            _fileLogger.IsActive = false;
            if (_cmdArgsParser.ParameterIsSet("lf"))
            {
                var logFileFullPath = _cmdArgsParser.GetParameterValue("lf");
                if (!string.IsNullOrEmpty(logFileFullPath))
                {
                    var path = Path.GetDirectoryName(logFileFullPath);
                    if (path != null)
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        if (File.Exists(logFileFullPath))
                        {
                            File.Delete(logFileFullPath);
                        }

                        _fileLogger.IsActive = true;
                        _fileLogger.LogFileName = logFileFullPath;

                        _logger.Log(string.Format("Logging to: {0}", logFileFullPath));
                    }
                }
            }
        }

        private static IList<string> GetListOfAssembliesFromParameter(string parameterWithCommaSeparatedListOfDllPaths)
        {
            var items = parameterWithCommaSeparatedListOfDllPaths.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);

            return items;
        }

        private void OnStarted(object sender, TestExecutorStartedEventArgs e)
        {
            _logger.Log(string.Format("Lighthouse v{0} (c) 2011 - Remote Unit Test Run Started.",  Constants.Versions.ConsoleRunnerVersionText));
            _logger.Log(string.Format("Total Test Assemblies: {0} Total Test Methods: {1}.",
                                      e.TestRunStartedInformation.TotalNumberOfAssemblies,
                                      e.TestRunStartedInformation.TotalNumberOfMethods));

            TestRunInformation = e.TestRunStartedInformation;
        }

        private LighthouseUnitTestRunStartInformation _testRunInformation = new LighthouseUnitTestRunStartInformation();
        protected LighthouseUnitTestRunStartInformation TestRunInformation
        {
            get { return _testRunInformation; }
            set { _testRunInformation = value; }
        }

        private void OnTimedout(RemoteTestExecutionResults executionResults)
        {
            _logger.Log(string.Format("Exiting (-1) because execution Timed Out while waiting for test result."));
            _logger.Log(string.Format("Methods that did not had execution results when timeout occured:"));
            foreach (var methodInfo in executionResults.MethodsStartedButNotFinished)
            {
                _logger.Log(string.Format("{0}.{1}", methodInfo.Class.Name, methodInfo.Method.MethodName));
            }
            Environment.Exit(-1);
        }

        private void OnFinished(RemoteTestExecutionResults executionResults)
        {
            var thereWereAnyTests = executionResults.UnitTestOutcome.TestResults.Any();
            var totalPassed = executionResults.UnitTestOutcome.TestResults.Count(p => p.Result == UnitTestOutcome.Passed);
            var totalFailed = executionResults.UnitTestOutcome.TestResults.Count(p => p.Result != UnitTestOutcome.Passed);

            _logger.Log(string.Format("Total Tests: {0} | Tests Passed: {1}. | Tests Failed: {2}",TestRunInformation.TotalNumberOfMethods,  totalPassed, totalFailed));

            if (executionResults.ExecutionErrors.Any())
            {
                _logger.Log("Errors occured during Remote Silverlight unit test execution:");
                foreach (var executionError in executionResults.ExecutionErrors)
                {
                    _logger.Log(string.Format("\t {0}", executionError));
                }
                _logger.Log("");
            }

            if (!thereWereAnyTests)
            {
                _logger.Log("Exiting (-1) because no Unit Tests were executed - this can't be right, right?");
                Environment.Exit(-1);          
            }
            else if (executionResults.Aborted)
            {
                OnAborted(executionResults);
            }
            else if (executionResults.IsTimedOut)
            {
                OnTimedout(executionResults);
            }
            else if (totalFailed > 0)
            {
                _logger.Log("Exiting (-1) because some of the tests failed.");
                Environment.Exit(-1);               
            }
            else
            {
                _logger.Log("Lighthouse Test Run completed successfully.");
                Environment.Exit(0);               
            }
        }

        private void OnAborted(RemoteTestExecutionResults executionResults)
        {
            _logger.Log(string.Format("Exiting (-1) because execution was Aborted."));
            Environment.Exit(-1);
        }

        public void Cleanup()
        {
            if (_skipCleanup)
            {
                _logger.Log("Skipping cleanup - temporary files will not be deleted.");
                return;
            }

            _cleanupManager.Cleanup();
        }

        public void Abort()
        {          
            _logger.Log("Aborting...");
            if (_currentTestExecutor != null)
            {
                _currentTestExecutor.Abort();
            }

            Cleanup();
        }
    }
}