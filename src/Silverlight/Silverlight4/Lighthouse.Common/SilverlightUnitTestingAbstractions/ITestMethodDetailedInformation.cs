namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface ITestMethodDetailedInformation
    {
        IUnitTestClass Class { get; set; }
        IUnitTestMethod Method { get; set; }
    }

    public class TestMethodDetailedInformation : ITestMethodDetailedInformation
    {
        public IUnitTestClass Class { get; set; }
        public IUnitTestMethod Method { get; set; }
    }
}