using System.Collections.Generic;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using LighthouseDesktop.Core.Infrastructure.XapManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public class RemoteTestExecutionResults
    {
        private ComposedUnitTestOutcome _unitTestOutcome = new ComposedUnitTestOutcome();
        public ComposedUnitTestOutcome UnitTestOutcome
        {
            get { return _unitTestOutcome; }
            set { _unitTestOutcome = value; }
        }

        private IList<ITestMethodDetailedInformation> _methodsStartedButNotFinished = new List<ITestMethodDetailedInformation>();
        public IList<ITestMethodDetailedInformation> MethodsStartedButNotFinished
        {
            get { return _methodsStartedButNotFinished; }
            set { _methodsStartedButNotFinished = value; }
        }

        public bool IsTimedOut { get; set; }

        public bool Aborted { get; set; }

        public bool RunWasComplete { get; set; }

        private IList<string> _errors = new List<string>();
        public IList<string> ExecutionErrors
        {
            get { return _errors; }
            set { _errors = value; }
        }

        public IXapBuildResult XapBuildResult { get; set; }
    }
}