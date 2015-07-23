using Microsoft.Silverlight.Testing.Harness;

namespace Lighthouse.Silverlight.Core.SilverlightUnitTestingCustomizations
{
    public class LighthouseUnitTestHarness : UnitTestHarness
    {
        protected override TestRunFilter CreateTestRunFilter(Microsoft.Silverlight.Testing.UnitTestSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.TagExpression))
            {
                return new TagTestRunFilter(settings, this, settings.TagExpression);
            }
            return new TestRunFilter(settings, this);

        }
    }
}