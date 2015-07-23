using System;
using System.IO;
using System.Runtime.InteropServices;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.Ioc;
using Lighthouse.Common.Services;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using LighthouseDesktop.Core.Infrastructure.TestResultsConverters;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public interface ITestResultsInformer
    {
        EventHandler<TestMethodDetailedInformationEventArgs> TestMethodStarted { get; set; }
        EventHandler<TestMethodFinishedEventArgs> TestMethodFinished { get; set; }
        EventHandler<TestsExecutionStartedEventArgs> TestsExecutionStarted { get; set; }
        EventHandler<TestAssemblyInformationEventArgs> TestAssemblyStarted { get; set; }
        EventHandler<TestAssemblyInformationEventArgs> TestAssemblyFinished { get; set; }
        EventHandler<TestsExecutionFinishedEventArgs> TestsExecutionFinished { get; set; }
        EventHandler<LogMessageSentFromTestingEnvironmentEventArgs> LogMessageSentFromTestingEnvironment { get; set; }
        EventHandler<EventArgs> SilverlightTestingClientReady { get; set; }
        EventHandler<TestingErrorOccuredEventArgs> TestingErrorOccured { get; set; }
    }


    public class TestsExecutionFinishedEventArgs : EventArgs
    {
        public ComposedUnitTestOutcome Results { get; set; }

        public TestsExecutionFinishedEventArgs(ComposedUnitTestOutcome results)
        {
            Results = results;
        }
    }

    public class TestsExecutionStartedEventArgs : EventArgs
    {
        private LighthouseUnitTestRunStartInformation _testRunStartInformation = new LighthouseUnitTestRunStartInformation();
        public LighthouseUnitTestRunStartInformation TestRunStartInformation
        {
            get { return _testRunStartInformation; }
            set { _testRunStartInformation = value; }
        }

        public TestsExecutionStartedEventArgs()
        {
        }

        public TestsExecutionStartedEventArgs(LighthouseUnitTestRunStartInformation testRunStartedEventArgs)
        {
            TestRunStartInformation = testRunStartedEventArgs;
        }
    }

    public class TestAssemblyInformationEventArgs : EventArgs
    {
        public IUnitTestAssembly Assembly { get; set; }

        public TestAssemblyInformationEventArgs(IUnitTestAssembly assembly)
        {
            Assembly = assembly;
        }
    }

    public class TestMethodDetailedInformationEventArgs : EventArgs
    {
        public ITestMethodDetailedInformation MethodDetailedInformation { get; set; }

        public TestMethodDetailedInformationEventArgs(ITestMethodDetailedInformation methodDetailedInformation)
        {
            MethodDetailedInformation = methodDetailedInformation;
        }
    }

    public class TestMethodFinishedEventArgs : EventArgs
    {
        public IUnitTestScenarioResult Result { get; set; }

        public TestMethodFinishedEventArgs(IUnitTestScenarioResult result)
        {
            Result = result;
        }
    }

    public class TestingErrorOccuredEventArgs : EventArgs
    {
        public IUnitTestException Exception { get; set; }     

        public TestingErrorOccuredEventArgs(IUnitTestException exception)
        {
            Exception = exception;
        }
    }

    public class LogMessageSentFromTestingEnvironmentEventArgs : EventArgs
    {
        public string Message{ get; set; }

        public LogMessageSentFromTestingEnvironmentEventArgs(string message)
        {
            Message = message;
        }
    }


    [ComVisible(true)]
    public class TestResultsInformer : ITestResultsInformer
    {
        private readonly ISerializationService _serializationService;

        public TestResultsInformer(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        public EventHandler<TestMethodDetailedInformationEventArgs> TestMethodStarted { get; set; }
        public EventHandler<TestMethodFinishedEventArgs> TestMethodFinished { get; set; }
        public EventHandler<TestsExecutionStartedEventArgs> TestsExecutionStarted { get; set; }
        public EventHandler<TestAssemblyInformationEventArgs> TestAssemblyStarted { get; set; }
        public EventHandler<TestAssemblyInformationEventArgs> TestAssemblyFinished { get; set; }
        public EventHandler<TestsExecutionFinishedEventArgs> TestsExecutionFinished { get; set; }
        public EventHandler<LogMessageSentFromTestingEnvironmentEventArgs> LogMessageSentFromTestingEnvironment { get; set; }
        public EventHandler<EventArgs> SilverlightTestingClientReady { get; set; }
        public EventHandler<TestingErrorOccuredEventArgs> TestingErrorOccured { get; set; }

        private void OnFinished(ComposedUnitTestOutcome results)
        {
            var handler = TestsExecutionFinished;
            if (handler != null)
            {
                handler(this, new TestsExecutionFinishedEventArgs(results));
            }
        }

        private void OnStarted(LighthouseUnitTestRunStartInformation info)
        {
            var handler = TestsExecutionStarted;
            if (handler != null)
            {
                handler(this, new TestsExecutionStartedEventArgs(info));
            }
        }

        public void TestsStarted(string serializedLighthouseUnitTestRunStartedEventArgs)
        {
            var convertedInformation = new LighthouseUnitTestRunStartInformation();
            try
            {
                convertedInformation = _serializationService.Deserialize<LighthouseUnitTestRunStartInformation>(serializedLighthouseUnitTestRunStartedEventArgs);                
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing data for TestsStarted: " + exc.Message);
            }

            OnStarted(convertedInformation);
        }

        public void TestMethodStarting(string serializedTestMethodDetailedInformation)
        {
            try
            {
                var convertedResults = _serializationService.Deserialize<TestMethodDetailedInformation>(serializedTestMethodDetailedInformation);

                var h = TestMethodStarted;
                if (h != null)
                {
                    h(this, new TestMethodDetailedInformationEventArgs(convertedResults));
                }
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing data : " + exc.Message);
            }
        }

        public void TestMethodCompleted(string serializedUnitTestScenarioResult)
        {
            try
            {
                var convertedResults = _serializationService.Deserialize<UnitTestScenarioResult>(serializedUnitTestScenarioResult);

                var h = TestMethodFinished;
                if (h != null)
                {
                    h(this, new TestMethodFinishedEventArgs(convertedResults));
                }
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing data : " + exc.Message);
            }
        }

        public void TestsAssemblyStarting(string serializedAssemblyData)
        {
            try
            {
                IUnitTestAssembly convertedResults = _serializationService.Deserialize<UnitTestAssembly>(serializedAssemblyData);

                var h = TestAssemblyStarted;
                if (h != null)
                {
                    h(this, new TestAssemblyInformationEventArgs(convertedResults));
                }
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing data : " + exc.Message);
            }
        }

        public void TestsAssemblyFinished(string serializedAssemblyData)
        {
            try
            {
                IUnitTestAssembly convertedResults = _serializationService.Deserialize<UnitTestAssembly>(serializedAssemblyData);

                var h = TestAssemblyFinished;
                if (h != null)
                {
                    h(this, new TestAssemblyInformationEventArgs(convertedResults));
                }
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing data : " + exc.Message);
            }
        }

        public void TestsFinished(string result)
        {
            var convertedResults = new ComposedUnitTestOutcome();
            try
            {
                convertedResults = _serializationService.Deserialize<ComposedUnitTestOutcome>(result);
            }
            catch (Exception exc)
            {
                OnErrorOccured("Error while deserializing Test Run Results: " + exc.Message);
            }

            OnFinished(convertedResults);
        }

        public void ReadyToStart()
        {
            OnReady();
        }

        public void LogMessageSent(string message)
        {
            var h = LogMessageSentFromTestingEnvironment;
            if (h != null)
            {
                h(this, new LogMessageSentFromTestingEnvironmentEventArgs(message));
            }
        }

        public void ClientLogMessage(string message)
        {
            LogMessageSent(message);
        }


        public void ErrorOccured(string serializedException)
        {
            IUnitTestException materializedException = null;

            try
            {
                materializedException = _serializationService.Deserialize<IUnitTestException>(serializedException);
            }
            catch
            {
                // if we cannot deserialize it send it as message
                OnErrorOccured(serializedException);
            }

            OnErrorOccured(materializedException);
        }

        private void OnErrorOccured(IUnitTestException exception)
        {
            var h = TestingErrorOccured;
            if (h != null)
            {
                h(this, new TestingErrorOccuredEventArgs(exception));
            }
        }

        private void OnErrorOccured(string errorMessage)
        {
            var h = TestingErrorOccured;
            if (h != null)
            {
                h(this, new TestingErrorOccuredEventArgs(new UnitTestException() {Message = errorMessage}));
            }
        }

        public void OnReady()
        {
            var h = SilverlightTestingClientReady;
            if (h != null)
            {
                SilverlightTestingClientReady(this, EventArgs.Empty);
            }
        }
    }
}