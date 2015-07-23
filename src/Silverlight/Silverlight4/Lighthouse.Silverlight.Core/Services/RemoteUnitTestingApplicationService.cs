using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Ioc;
using Lighthouse.Common.Services;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using Lighthouse.Silverlight.Core.Controls;
using Lighthouse.Silverlight.Core.SilverlightUnitTestingCustomizations;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using System.Linq;

namespace Lighthouse.Silverlight.Core.Services
{
    [ScriptableType]
    public class RemoteUnitTestingApplicationService : IApplicationService, IApplicationLifetimeAware
    {
        private LighthouseUnitTestRunnerPage _testPage;

        private static RemoteUnitTestingApplicationService _current;
        public static RemoteUnitTestingApplicationService Current
        {
            get { return _current; }
        }

        private ISerializationService _serializationService;
        private ISilverlightUnitTestAbstractionsFactory _silverlightUnitTestAbstractionsFactory;

        public RemoteUnitTestingApplicationService()
        {
            _silverlightUnitTestAbstractionsFactory =
                SimpleServiceLocator.Instance.Get<ISilverlightUnitTestAbstractionsFactory>();

            _serializationService = SimpleServiceLocator.Instance.Get<ISerializationService>();
        }

        public RemoteUnitTestingApplicationService(ISerializationService serializationService, ISilverlightUnitTestAbstractionsFactory silverlightUnitTestAbstractionsFactory)
        {
            _serializationService = serializationService;
            _silverlightUnitTestAbstractionsFactory = silverlightUnitTestAbstractionsFactory;
        }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public void StartService(ApplicationServiceContext context)
        {
            _serializationService = new SerializationService();
            _silverlightUnitTestAbstractionsFactory = new SilverlightUnitTestAbstractionsFactory();

            _current = this;

            if (IsEnabled)
            {
                HtmlPage.RegisterScriptableObject("TestFrontend", this);

                var waitingPage = new WaitingPage();
                waitingPage.Loaded += (s, e) => InvokeExternalMethod("ReadyToStart");

                Application.Current.RootVisual = waitingPage;
            }
        }

        public void Run(SilverlightUnitTestRunSettings settings)
        {
            UnitTestSettings unitTestSettings = MyUnitTestSystem.CreateDefaultSettings(settings);

            _testPage = MyUnitTestSystem.CreateTestPage(unitTestSettings) as LighthouseUnitTestRunnerPage;

            if (_testPage != null)
            {
                var userControl = Application.Current.RootVisual as UserControl;

                if (userControl != null)
                {
                    userControl.Content = _testPage;
                    userControl.Focus();

                    try
                    {
                        _testPage.StartTesting();
                    }
                    catch (Exception e)
                    {
                        ReportError(e);
                        PublishResultsWithError(e);
                    }
                }
                else
                {
                    ReportError(new Exception("Could not assign new RootVisual"));
                }
            }
        }


        public void Run()
        {
            Run(new SilverlightUnitTestRunSettings()
            {
                AssembliesThatContainTests = new List<string>() { "Lighthouse.Silverlight4.SampleXapWithTests.dll" }
            });
        }

        [ScriptableMember]
        public void RemoteRun(string settings)
        {
            var materializedSettings = new SilverlightUnitTestRunSettings();

            try
            {
                materializedSettings = _serializationService.Deserialize<SilverlightUnitTestRunSettings>(settings);
            }
            catch (Exception)
            {
                                
            }

            Run(materializedSettings);
        }

        private bool InvokeExternalMethod(string name)
        {
            return InvokeExternalMethod(name, new object[] {});
        }

        private bool InvokeExternalMethod(string name, object[] parameters)
        {
            var testResultsInformer = HtmlPage.Window.Eval("window.external") as ScriptObject;
            if (testResultsInformer != null)
            {
                try
                {
                    testResultsInformer.Invoke(name, parameters);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        private bool InvokeExternalMethod(string name, string parameters)
        {
            return InvokeExternalMethod(name, new object[] {parameters});
        }

        public void OnTestRunStarting(object sender, TestRunStartingEventArgs testRunStartingEventArgs)
        {
            var args = new LighthouseUnitTestRunStartInformation()
                           {
                               TotalNumberOfAssemblies = testRunStartingEventArgs.EnqueuedAssemblies,
                               TotalNumberOfMethods = testRunStartingEventArgs.Settings.TestHarness.TestMethodCount
                           };

            var serializedResult = _serializationService.Serialize(args);

            InvokeExternalMethod("TestsStarted", serializedResult);
        }

        public void ReportThatWeStartedExecutingTests(string count)
        {
            InvokeExternalMethod("TestsStarted", count);
        }

        public void OnTestClassStarting(object sender, TestClassStartingEventArgs e)
        {

        }

        public void OnTestClassCompleted(object sender, TestClassCompletedEventArgs e)
        {

        }

        public void OnTestMethodStarting(object sender, TestMethodStartingEventArgs e)
        {
            var result = new TestMethodDetailedInformation()
                             {
                                 Class = _silverlightUnitTestAbstractionsFactory.Convert(e.TestClass),
                                 Method = _silverlightUnitTestAbstractionsFactory.Convert(e.TestMethod)
                             };

            var serializedResult = _serializationService.Serialize(result);

            InvokeExternalMethod("TestMethodStarting", serializedResult);
        }

        public void OnTestMethodCompleted(object sender, TestMethodCompletedEventArgs e)
        {
            var convertedResult = _silverlightUnitTestAbstractionsFactory.Convert(e.Result);
            var serializedResult = _serializationService.Serialize(convertedResult);

            InvokeExternalMethod("TestMethodCompleted", serializedResult);
        }

        public void PublishResults(List<ScenarioResult> results)
        {
            var convertedResult = _silverlightUnitTestAbstractionsFactory.Convert(results);
            var serializedResult = _serializationService.Serialize(convertedResult);

            InvokeExternalMethod("TestsFinished", serializedResult);
        }

        public void PublishResultsWithError(Exception error)
        {
            var errorInfo = _silverlightUnitTestAbstractionsFactory.Convert(error);
            var results = new ComposedUnitTestOutcome() {Errors = new List<IUnitTestException>() {errorInfo}};

            var serializedResult = _serializationService.Serialize(results);

            InvokeExternalMethod("TestsFinished", serializedResult);
        }

        public void StopService()
        {
            
        }

        public void Starting()
        {
            
        }

        public void Started()
        {

        }

        public void Exiting()
        {
            
        }

        public void Exited()
        {
            
        }

        public void SendLogMessageToRemoteTestExecutor(string message)
        {
            InvokeExternalMethod("LogMessageSent", message);            
        }

        public void ReportError(Exception exception)
        {
            var convertedException = _silverlightUnitTestAbstractionsFactory.Convert(exception);
            var serializedException = _serializationService.Serialize(convertedException);

            InvokeExternalMethod("ErrorOccured", serializedException);
        }

        public void TestAssemblyStarting(object sender, TestAssemblyStartingEventArgs e)
        {
            var convertedData = _silverlightUnitTestAbstractionsFactory.Convert(e.Assembly);
            var serializedData = _serializationService.Serialize(convertedData);

            InvokeExternalMethod("TestsAssemblyStarting", serializedData);            
        }

        public void TestAssemblyCompleted(object sender, TestAssemblyCompletedEventArgs e)
        {
            var convertedData = _silverlightUnitTestAbstractionsFactory.Convert(e.Assembly);
            var serializedData = _serializationService.Serialize(convertedData);

            InvokeExternalMethod("TestsAssemblyCompleted", serializedData);              
        }
    }
}