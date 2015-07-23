using System;
using System.Reflection;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IUnitTestMethod
    {
        string MethodName { get; }
        string Name { get; }
    }

    public class UnitTestMethod : IUnitTestMethod
    {
        public string Name { get; set; }
        public string MethodName { get; set; }
    }
}