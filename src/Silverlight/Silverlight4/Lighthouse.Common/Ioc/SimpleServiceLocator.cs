using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Lighthouse.Common.Ioc
{
    public class SimpleServiceLocator : ISimpleServiceLocator
    {
        private readonly Dictionary<Type, Type> _registeredTypes = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _registeredInstances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ICreationStrategy> _registeredCreationStrategies = new Dictionary<Type, ICreationStrategy>();

        private static ISimpleServiceLocator _currentInstance;
        public static ISimpleServiceLocator Instance
        {
            get
            {
                if (_currentInstance == null)
                {
                    _currentInstance = new SimpleServiceLocator();
                    _currentInstance.RegisterInstance(_currentInstance);
                }

                return _currentInstance;
            }
        }

        public TType Get<TType>()
        {
            var instance = MaterializeType(typeof (TType));
            if (instance == null)
            {
                throw new Exception(string.Format("Could not resolve dependancy: {0}.", typeof(TType)));               
            }
            return (TType) instance;
        }

        private object MaterializeType(Type type)
        {
            if (_registeredInstances.ContainsKey(type))
            {
                return _registeredInstances[type];
            }

            if (_registeredTypes.ContainsKey(type))
            {
                var implementationType = _registeredTypes[type];

                ICreationStrategy strategy;

                if (_registeredCreationStrategies.ContainsKey(type))
                {
                    strategy = _registeredCreationStrategies[type];

                    var instance = strategy.Create(implementationType,
                                                   MaterializeConstructorParameters(implementationType));

                    return instance;
                }
            }

            return null;
        }

        private object[] MaterializeConstructorParameters(Type implementationType)
        {
            var constructor = FindConstructorToUseForInjection(implementationType);

            var listOfParametersToMaterialize = constructor.GetParameters();
            var materializedParameters = new List<object>(constructor.GetParameters().Length);

            foreach (var parameterInfo in listOfParametersToMaterialize)
            {
                var materializedParameterInstance = MaterializeType(parameterInfo.ParameterType);

                if (materializedParameterInstance == null)
                {
                    throw new Exception(string.Format("Could not resolve dependancy: {0} for constructor of type {1}", parameterInfo.ParameterType.Name, implementationType.Name));
                }

                materializedParameters.Add(materializedParameterInstance);
            }

            return materializedParameters.ToArray();
        }

        private static ConstructorInfo FindConstructorToUseForInjection(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();

            if (!constructors.Any())
            {
                throw new Exception(string.Format("Type {0} has no constructors", implementationType.Name));
            }

            var constructorWithLongestSignature = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            return constructorWithLongestSignature;
        }

        public void Register<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _registeredTypes.Add(typeof(TInterface), typeof(TImplementation));
            _registeredCreationStrategies.Add(typeof(TInterface), new DefaultCreationStrategy());
        }

        public void Register<TInterface, TImplementation>(ICreationStrategy creationStrategy) where TImplementation : TInterface
        {
            _registeredTypes.Add(typeof(TInterface), typeof(TImplementation));
            _registeredCreationStrategies.Add(typeof(TInterface), creationStrategy);
        }

        public void RegisterInstance<TInterface>(TInterface instance)
        {
            _registeredInstances.Add(typeof(TInterface), instance);
        }

        public bool IsRegistered<TInterface>()
        {
            var desiredType = typeof (TInterface);

            if (_registeredInstances.ContainsKey(desiredType))
            {
                return true;
            }

            if (_registeredTypes.ContainsKey(desiredType))
            {
                return true;
            }

            return false;
        }
    }
}