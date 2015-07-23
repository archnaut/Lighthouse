using System;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IUnitTestException
    {
        string Message { get; }
        string StackTrace { get; }
    }

    public class UnitTestException : IUnitTestException
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}