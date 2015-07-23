using System;
using System.Collections.Generic;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace Lighthouse.Silverlight.Core.Services
{
    public interface ISilverlightUnitTestAbstractionsFactory
    {
        IUnitTestScenarioResult Convert(ScenarioResult source);
        IUnitTestException Convert(Exception source);
        IUnitTestMethod Convert(ITestMethod source);
        IUnitTestClass Convert(ITestClass source);
        IUnitTestAssembly Convert(IAssembly source);
        IComposedUnitTestOutcome Convert(IEnumerable<ScenarioResult> source);
    }

    public class SilverlightUnitTestAbstractionsFactory : ISilverlightUnitTestAbstractionsFactory
    {
        public IUnitTestScenarioResult Convert(ScenarioResult source)
        {
            var result = new UnitTestScenarioResult()
                             {
                                 Exception = Convert(source.Exception),
                                 Finished = source.Finished,
                                 Result = (UnitTestOutcome) source.Result,
                                 Started = source.Started,
                                 TestClass = Convert(source.TestClass),
                                 TestMethod = Convert(source.TestMethod)

                             };

            return result;
        }

        public IUnitTestException Convert(Exception source)
        {
            if (source == null)
            {
                return null;
            }

            return new UnitTestException()
                       {
                           Message = source.Message,
                           StackTrace = source.StackTrace
                       };
        }

        public IUnitTestMethod Convert(ITestMethod source)
        {
            return new UnitTestMethod()
                       {
                           MethodName = source.Method.Name,
                           Name = source.Name
                       };
        }

        public IUnitTestClass Convert(ITestClass source)
        {
            return new UnitTestClass()
                       {
                           Name = source.Name,
                           TypeName = source.Type.Name,
                           Assembly = Convert(source.Assembly),
                           Namespace = source.Type.Namespace
                       };

        }

        public IUnitTestAssembly Convert(IAssembly source)
        {
            return new UnitTestAssembly()
                       {
                           Name = source.Name
                       };
        }

        public IComposedUnitTestOutcome Convert(IEnumerable<ScenarioResult> source)
        {
            var result = new ComposedUnitTestOutcome();

            foreach (var scenarioResult in source)
            {
                result.TestResults.Add(Convert(scenarioResult));
            }

            return result;
        }
    }
}