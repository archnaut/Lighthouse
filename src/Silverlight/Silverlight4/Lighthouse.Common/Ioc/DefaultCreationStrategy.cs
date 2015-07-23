using System;

namespace Lighthouse.Common.Ioc
{
    public class DefaultCreationStrategy : ICreationStrategy
    {
        public object Create(Type type, object[] constructorParameters)
        {
            return Activator.CreateInstance(type, constructorParameters);
        }
    }
}