using System;
using System.IO;
using System.Reflection;
using LighthouseDesktop.Core.Resources.Silverlight4Specific;

namespace LighthouseDesktop.Core.Infrastructure.ResourceManagement
{
    public interface ISilverlightVersionSpecificResourcesProvider 
    {
        int SilverlightVersion { get; set; }
        Stream GetResourceStream(string resourceName);
        string GetResourceContent(string resourceName);
    }

    public class SilverlightVersionSpecificResourcesProvider : MarkerTypeResourceManager, ISilverlightVersionSpecificResourcesProvider
    {
        private int _silverlightVersion = 4;
        public int SilverlightVersion
        {
            get { return _silverlightVersion; }
            set { _silverlightVersion = value; }
        }

        public SilverlightVersionSpecificResourcesProvider(ISimpleResourceManager simpleResourceManager) : base(simpleResourceManager)
        {
            
        }

        protected override Type GetMarkerType()
        {
            if (SilverlightVersion == 4)
            {
                return typeof(Silverlight4SpecificResourcesNamespaceMarker);
            }

            throw new InvalidOperationException("Only version 4 of Silverlight is supported.");
        }
    }
}