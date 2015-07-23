using Lighthouse.Common.Ioc;
using Lighthouse.Common.Services;
using Lighthouse.Silverlight.Core.Services;

namespace Lighthouse.Silverlight.Core
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            SimpleServiceLocator.Instance.Register<ISilverlightUnitTestAbstractionsFactory, SilverlightUnitTestAbstractionsFactory>();
            SimpleServiceLocator.Instance.Register<ISerializationService, SerializationService>();
        }
    }
}