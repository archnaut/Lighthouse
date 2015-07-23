using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Ioc;
using Lighthouse.Common.Services;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using LighthouseDesktop.Core.Infrastructure.Logging;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;
using LighthouseDesktop.Core.Infrastructure.TestExecutionEnvironment;
using LighthouseDesktop.Core.Infrastructure.TestResultsConverters;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public class TestExecutorStartedEventArgs : EventArgs
    {
        public LighthouseUnitTestRunStartInformation TestRunStartedInformation { get; set; }

        public TestExecutorStartedEventArgs(LighthouseUnitTestRunStartInformation info)
        {
            TestRunStartedInformation = info;
        }
    }
   
    public abstract class TestExecutor : ITestExecutor
    {
        public EventHandler<TestExecutorStartedEventArgs> Started { get; set; }

        public string TestResultsOutputFileFullPath { get; set; }

        public string WorkDirectory { get; set; }


        private readonly IList<ITestMethodDetailedInformation> _methodsStarted = new List<ITestMethodDetailedInformation>();
        private readonly IList<ITestMethodDetailedInformation> _methodsFinished = new List<ITestMethodDetailedInformation>();

        private RemoteTestExecutionSettings _testExecutionSettings = new RemoteTestExecutionSettings();
        public RemoteTestExecutionSettings TestExecutionSettings
        {
            get { return _testExecutionSettings; }
            set { _testExecutionSettings = value; }
        }

        private RemoteTestExecutionResults _testExecutionResults = new RemoteTestExecutionResults();
        public RemoteTestExecutionResults TestExecutionResults
        {
            get { return _testExecutionResults; }
            set { _testExecutionResults = value; }
        }

        private const string HtmlPageName = "LighthouseSilverlightTestRunnerPage.html";

        public string HtmlTestPageFullPath
        {
            get { return Path.Combine(CurrentTestSessionTempDirectory, HtmlPageName); }
        }

        public virtual RemoteTestExecutionResults Execute()
        {
            if (TestExecutionSettings == null || TestExecutionSettings.XapUri == null)
            {
                return TestExecutionResults;
            }

            var htmlPageUri = _htmlPageBuilder.BuildHtmlPage(TestExecutionSettings.XapUri, HtmlTestPageFullPath);
            _webBrowser.Navigated += _webBrowser_Navigated;
            _webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(_webBrowser_DocumentCompleted);

            _watchdogTimer = new System.Threading.Timer(WatchdogCallback, null, TimeSpan.FromSeconds(TestExecutionSettings.TimeoutInSeconds), TimeSpan.FromMilliseconds(-1));  

            try
            {
                if (Aborted)
                {
                    SaveXmlResultsFileIfNeeded();
                    return TestExecutionResults; 
                }

                _webBrowser.Navigate(htmlPageUri);
            }
            catch (Exception e)
            {
                _logger.Log(string.Format("Error occured: {0}", e.Message));

                SaveXmlResultsFileIfNeeded();
                return TestExecutionResults;
            }

            Application.Run(_form);

            SaveXmlResultsFileIfNeeded();
            return TestExecutionResults;
        }

        public virtual void Abort()
        {
            Aborted = true;
            Finished = true;

            TestExecutionResults.Aborted = true;

            CloseTheTestingFormAndCleanup();
        }

        protected bool Aborted { get; set; }

        private static readonly object FinishedLocker = new object();

        private bool _finished;
        protected bool Finished
        {
            get { return _finished; }
            set
            {
                lock (FinishedLocker)
                {
                    _finished = value;                   
                }
            }
        }

        void _webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            e.Handled = true;
        }

        void _webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (_webBrowser.Document != null && _webBrowser.Document.Window != null)
            {
                _webBrowser.Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
            }
        }

        protected virtual void CloseTheTestingFormAndCleanup()
        {
            if (_watchdogTimer !=null)
            {
                _watchdogTimer.Dispose();                
            }

            if (_form != null && _form.IsHandleCreated)
            {
                _form.Invoke(new Action(() =>_form.Close()));
            }
        }

        private void OnInformerFinished(object sender, TestsExecutionFinishedEventArgs testsFinishedEventArgs)
        {
            if (Finished)
            {
                return;
            }

            Finished = true;
            TestExecutionResults.RunWasComplete = true;

            // overrwrite the temp test results since we have full test run
            TestExecutionResults.UnitTestOutcome = testsFinishedEventArgs.Results;

            CloseTheTestingFormAndCleanup();
        }

        private void SaveXmlResultsFileIfNeeded()
        {
            if (!string.IsNullOrEmpty(TestResultsOutputFileFullPath))
            {
                if (File.Exists(TestResultsOutputFileFullPath))
                {
                    File.Delete(TestResultsOutputFileFullPath);
                }

                try
                {
                    var xmlResultsFileCreator = SimpleServiceLocator.Instance.Get<INUnitXmlResultsFileCreator>();
                    var xml = xmlResultsFileCreator.CreateFile(TestExecutionResults);

                    var file = File.CreateText(TestResultsOutputFileFullPath);
                    file.Write(xml);
                    file.Flush();
                    file.Close();

                    _logger.Log(string.Format("Testing results saved to file: {0}", TestResultsOutputFileFullPath));
                }
                catch (Exception exc)
                {
                    _logger.Log(string.Format("Error while writing the results to file {0}. Error {1}", TestResultsOutputFileFullPath, exc.Message));
                }
            }
        }

        private void OnStarted(LighthouseUnitTestRunStartInformation info)
        {
            var handler = Started;
            if (handler != null)
            {
                handler(this, new TestExecutorStartedEventArgs(info));
            }
        }

        private readonly IHtmlPageBuilder _htmlPageBuilder;
        private readonly ISerializationService _serializationService;
        private readonly ITestResultsInformer _testResultsInformer;
        private readonly ILogger _logger;
        private readonly ICleanupManager _cleanupManager;

        private readonly WebBrowser _webBrowser;
        private readonly Form _form;
        private System.Threading.Timer _watchdogTimer;

        public Guid TestSessionId { get; private set; }


        public string CurrentTestSessionTempDirectory
        {
            get
            {
                var path = Path.Combine(WorkDirectory, TestSessionId.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string OutputXapFullPath
        {
            get { return Path.Combine(CurrentTestSessionTempDirectory, "LighthouseTestRun.xap"); }
        }

        protected TestExecutor(IHtmlPageBuilder htmlPageBuilder, ISerializationService serializationService, ITestResultsInformer testResultsInformer, ILogger logger, ICleanupManager cleanupManager)
        {
            _htmlPageBuilder = htmlPageBuilder;
            _serializationService = serializationService;
            _testResultsInformer = testResultsInformer;
            _logger = logger;
            _cleanupManager = cleanupManager;

            TestSessionId = Guid.NewGuid();

            WorkDirectory = Path.GetTempPath();

            if (!Directory.Exists(WorkDirectory))
            {
                Directory.CreateDirectory(WorkDirectory);
            }

            if (!Directory.Exists(CurrentTestSessionTempDirectory))
            {
                Directory.CreateDirectory(CurrentTestSessionTempDirectory);
            }
            _cleanupManager.DirectoriesToDelete.Add(CurrentTestSessionTempDirectory);

            _testResultsInformer.TestsExecutionFinished += OnInformerFinished;
            _testResultsInformer.TestsExecutionStarted += OnInformerStarted;
            _testResultsInformer.SilverlightTestingClientReady += OnRemoteTestEnvironmentReady;
            _testResultsInformer.TestingErrorOccured += OnInformerTestingErrorOccured;
            _testResultsInformer.LogMessageSentFromTestingEnvironment += (s, e) =>
                                                                             {
                                                                                 slideTheWatchdogTimer();
                                                                                 _logger.Log(e.Message);
                                                                             };
            _testResultsInformer.TestAssemblyStarted += (s, e) =>
                                                            {
                                                                slideTheWatchdogTimer();
                                                                _logger.Log(string.Format("Starting Assembly: {0}",
                                                                                          e.Assembly.Name));
                                                            };
            _testResultsInformer.TestAssemblyFinished += (s, e) =>
                                                             {
                                                                 slideTheWatchdogTimer();
                                                                 _logger.Log(string.Format("Finished Assembly: {0}",
                                                                                           e.Assembly.Name));
                                                             };
            _testResultsInformer.TestMethodStarted += (s, e) =>
                                                          {
                                                              slideTheWatchdogTimer();
                                                              _methodsStarted.Add(new TestMethodDetailedInformation() {Class = e.MethodDetailedInformation.Class, Method = e.MethodDetailedInformation.Method});
                                                              var msg = string.Format("[Started] {0}.{1}", e.MethodDetailedInformation.Class.Name, e.MethodDetailedInformation.Method.MethodName);
                                                              _logger.Log(msg);
                                                          };
            _testResultsInformer.TestMethodFinished += (s, e) =>
                                                           {
                                                               slideTheWatchdogTimer();
                                                               _methodsFinished.Add(new TestMethodDetailedInformation() { Class = e.Result.TestClass, Method = e.Result.TestMethod});
                                                               TestExecutionResults.UnitTestOutcome.TestResults.Add(e.Result);
                                                               var msg = new StringBuilder(string.Format("{0}.{1} [{2}] ",e.Result.TestClass.Name, e.Result.TestMethod.MethodName, e.Result.Result.ToString()));
                                                               if (e.Result.Result != UnitTestOutcome.Passed && e.Result.Exception != null)
                                                               {
                                                                   msg.AppendLine(e.Result.Exception.Message);
                                                               }

                                                               _logger.Log(msg.ToString());
                                                           };

            _webBrowser = new WebBrowser
                              {
                                  Dock = DockStyle.Fill,
                                  ObjectForScripting = _testResultsInformer,
                                  AllowNavigation = true,
                                  ScriptErrorsSuppressed = true
            };

            _form = new Form
            {
                Text = "SilverlightUnitTestRunner",
                Controls = { _webBrowser },
                Visible = false,
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false
            };
        }

        private void OnInformerTestingErrorOccured(object sender, TestingErrorOccuredEventArgs testingErrorOccuredEventArgs)
        {
            slideTheWatchdogTimer();
            TestExecutionResults.ExecutionErrors.Add(string.Format("{0} @ {1}", testingErrorOccuredEventArgs.Exception.Message, testingErrorOccuredEventArgs.Exception.StackTrace));
        }

        private void OnRemoteTestEnvironmentReady(object sender, EventArgs e)
        {
            slideTheWatchdogTimer();

            if (_webBrowser.Document != null)
            {
                Thread.Sleep(200);

                var slSettings = _serializationService.Serialize(TestExecutionSettings.SilverlightUnitTestRunSettings);

                _webBrowser.Document.InvokeScript("startTesting", new[] {slSettings});
            }
        }

        private void slideTheWatchdogTimer()
        {
            if (_watchdogTimer != null)
            {
                _watchdogTimer.Change(TimeSpan.FromSeconds(TestExecutionSettings.TimeoutInSeconds), TimeSpan.FromMilliseconds(-1));
            }            
        }

        private void OnInformerStarted(object sender, TestsExecutionStartedEventArgs testsStartedEventArgs)
        {
            OnStarted(testsStartedEventArgs.TestRunStartInformation);

            slideTheWatchdogTimer();
        }

        private void WatchdogCallback(object state)
        {
            if (Finished)
            {
                return;
            }

            Finished = true;

            var methodsStartedCopy = _methodsStarted.ToArray();
            var methodsFinishedCopy = _methodsFinished.ToArray();

            var methodsNotFinished =
                methodsStartedCopy.Where(mf => !methodsFinishedCopy.Any(ms => mf.Method.MethodName == ms.Method.MethodName && mf.Class.TypeName == ms.Class.TypeName))
                    .ToList();

            TestExecutionResults.MethodsStartedButNotFinished = methodsNotFinished;
            TestExecutionResults.IsTimedOut = true;
            TestExecutionResults.RunWasComplete = false;

            CloseTheTestingFormAndCleanup();
        }
    }


    public interface ITestExecutor
    {
        void Abort();

        RemoteTestExecutionSettings TestExecutionSettings { get; set; }

        RemoteTestExecutionResults TestExecutionResults { get; set; }

        string TestResultsOutputFileFullPath { get; set; }

        string WorkDirectory { get; set; }

        RemoteTestExecutionResults Execute();

        EventHandler<TestExecutorStartedEventArgs> Started { get; set; }
    }
}