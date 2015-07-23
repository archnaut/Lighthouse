using System;

namespace Lighthouse.Common.Ioc
{
    public interface ICreationStrategy
    {
        object Create(Type type, object[] constructorParameters);
    }
}