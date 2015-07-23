using System;
using System.IO;

namespace LighthouseDesktop.Core.Infrastructure.ResourceManagement
{
    public abstract class MarkerTypeResourceManager
    {
        protected abstract Type GetMarkerType();

        private readonly ISimpleResourceManager _simpleResourceManager;

        protected MarkerTypeResourceManager(ISimpleResourceManager simpleResourceManager)
        {
            _simpleResourceManager = simpleResourceManager;
        }

        private Type CurrentMarkerType
        {
            get { return GetMarkerType(); }
        }

        public Stream GetResourceStream(string resourceName)
        {
            return _simpleResourceManager.GetResourceStream(CurrentMarkerType.Assembly, CurrentMarkerType, resourceName);
        }

        public string GetResourceContent(string resourceName)
        {
            return _simpleResourceManager.GetResourceContent(CurrentMarkerType.Assembly, CurrentMarkerType, resourceName);
        }

    }
}