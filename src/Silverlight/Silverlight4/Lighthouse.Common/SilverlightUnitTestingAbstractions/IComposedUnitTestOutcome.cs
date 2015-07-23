using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IComposedUnitTestOutcome
    {
        IList<IUnitTestScenarioResult> TestResults { get; set; }
        IList<IUnitTestException> Errors { get; set; }
    }

    public class ComposedUnitTestOutcome : IComposedUnitTestOutcome
    {
        public ComposedUnitTestOutcome()
        {
        }

        private IList<IUnitTestScenarioResult> _testResults = new List<IUnitTestScenarioResult>();
        public IList<IUnitTestScenarioResult> TestResults
        {
            get { return _testResults; }
            set { _testResults = value; }
        }

        private IList<IUnitTestException> _errors = new List<IUnitTestException>();
        public IList<IUnitTestException> Errors
        {
            get { return _errors; }
            set { _errors = value; }
        }
    }
}