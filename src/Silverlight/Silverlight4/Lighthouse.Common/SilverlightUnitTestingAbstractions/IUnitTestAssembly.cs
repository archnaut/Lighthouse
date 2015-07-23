using System;

namespace Lighthouse.Common.SilverlightUnitTestingAbstractions
{
    public interface IUnitTestAssembly
    {
        string Name { get; }
    }

    public class UnitTestAssembly : IUnitTestAssembly
    {
        public string Name { get; set; }
    }
}