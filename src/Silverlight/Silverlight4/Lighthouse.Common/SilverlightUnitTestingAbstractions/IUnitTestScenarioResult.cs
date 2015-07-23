using System;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IUnitTestScenarioResult
    {
        DateTime Started { get; }
        DateTime Finished { get; }
        UnitTestOutcome Result { get; }
        IUnitTestException Exception { get; }
        IUnitTestMethod TestMethod { get; }
        IUnitTestClass TestClass { get; }
    }

    public class UnitTestScenarioResult : IUnitTestScenarioResult
    {
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public UnitTestOutcome Result { get; set; }
        public IUnitTestException Exception { get; set; }
        public IUnitTestMethod TestMethod { get; set; }
        public IUnitTestClass TestClass { get; set; }
    }
}