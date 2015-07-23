using System;
using System.IO;
using LighthouseDesktop.Core.Resources;

namespace LighthouseDesktop.Core.Infrastructure.ResourceManagement
{
    public interface IGenericResourcesProvider
    {
        Stream GetResourceStream(string resourceName);
        string GetResourceContent(string resourceName);
    }

    public class GenericResourcesProvider : MarkerTypeResourceManager, IGenericResourcesProvider
    {
        public GenericResourcesProvider(ISimpleResourceManager simpleResourceManager) : base(simpleResourceManager)
        {
        }

        protected override Type GetMarkerType()
        {
            return typeof(GenericResourcesNamespaceMarker);
        }
    }
}