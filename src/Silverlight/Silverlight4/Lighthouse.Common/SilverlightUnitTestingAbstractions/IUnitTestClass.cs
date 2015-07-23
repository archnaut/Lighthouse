using System;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IUnitTestClass
    {
        IUnitTestAssembly Assembly { get; }
        string Name { get; }
        string TypeName { get; }
        string Namespace { get; }
    }

    public class UnitTestClass : IUnitTestClass
    {
        public IUnitTestAssembly Assembly { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Namespace { get; set; }
    }
}