using System;
using System.IO;
using System.Reflection;

namespace LighthouseDesktop.Core.Infrastructure.ResourceManagement
{
    public interface ISimpleResourceManager
    {
        Stream GetResourceStream(Assembly assembly, Type markerType, string resourceName);
        string GetResourceContent(Assembly assembly, Type markerType, string resourceName);
    }

    public class SimpleResourceManager : ISimpleResourceManager
    {
        public Stream GetResourceStream(Assembly assembly, Type markerType, string resourceName)
        {
            var @namespace = markerType.Namespace;

            var fullResourceName = string.Format("{0}.{1}", @namespace, resourceName);

            var stream = assembly.GetManifestResourceStream(fullResourceName);

            if (stream == null)
            {
                throw new Exception("Could not load resource " + resourceName);
            }

            return stream;
        }

        public string GetResourceContent(Assembly assembly, Type markerType, string resourceName)
        {
            return new StreamReader(GetResourceStream(assembly, markerType, resourceName)).ReadToEnd();
        } 
    }
}