using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace Lighthouse.Silverlight.Core.SilverlightUnitTestingCustomizations
{
    public class MethodNameTestRunFilter : TestRunFilter
    {
        public MethodNameTestRunFilter(UnitTestSettings settings, UnitTestHarness harness) : base(settings, harness)
        {
            
        }

        public string MethodNameFilter { get; set; }

        public override List<ITestClass> GetTestClasses(IAssembly assembly, TestClassInstanceDictionary instances)
        {
            return base.GetTestClasses(assembly, instances);
        }

        protected override void FilterExclusiveTestMethods(IList<ITestMethod> methods)
        {
            base.FilterExclusiveTestMethods(methods);
        }

        protected override void FilterCustomTestMethods(IList<ITestMethod> methods)
        {
            if (string.IsNullOrWhiteSpace(MethodNameFilter))
            {
                // no filtering
                return;
            }

            var original = new List<ITestMethod>(methods);
            methods.Clear();

            foreach (ITestMethod method in original)
            {
                if (method.Name.Contains(MethodNameFilter) && original.Contains(method))
                {
                    methods.Add(method);
                }
            }
        }
    }
}