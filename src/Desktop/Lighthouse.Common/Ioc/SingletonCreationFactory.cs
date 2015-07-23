using System;
using System.Collections.Generic;

namespace Lighthouse.Common.Ioc
{
    public class SingletonCreationFactory : ICreationStrategy
    {
        private static readonly Dictionary<Type, object> Singletons = new Dictionary<Type, object>();
        private static readonly ICreationStrategy DefaultCreationStrategy = new DefaultCreationStrategy();

        public object Create(Type type, object[] constructorParameters)
        {
            if (!Singletons.ContainsKey(type))
            {
                Singletons.Add(type, DefaultCreationStrategy.Create(type, constructorParameters));
            }

            return Singletons[type];
        }
    }
}