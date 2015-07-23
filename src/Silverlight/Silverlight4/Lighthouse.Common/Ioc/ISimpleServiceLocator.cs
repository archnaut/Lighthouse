namespace Lighthouse.Common.Ioc
{
    public interface ISimpleServiceLocator
    {
        TType Get<TType>();
        void Register<TInterface, TImplementation>() where TImplementation : TInterface;
        void Register<TInterface, TImplementation>(ICreationStrategy creationStrategy) where TImplementation : TInterface;
        void RegisterInstance<TInterface>(TInterface instance);
        bool IsRegistered<TInterface>();
    }
}