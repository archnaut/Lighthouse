using System;
using System.IO;
using System.Resources;
using LighthouseDesktop.Core.Infrastructure.ResourceManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestExecutionEnvironment
{
    public interface IHtmlPageBuilder
    {
        Uri BuildHtmlPage(Uri xapUri, string htmlPageFullPath);
    }

    public class HtmlPageBuilder : IHtmlPageBuilder
    {

        private readonly IGenericResourcesProvider _genericResourcesProvider;

        public HtmlPageBuilder(IGenericResourcesProvider genericResourcesProvider)
        {
            _genericResourcesProvider = genericResourcesProvider;
        }

        public Uri BuildHtmlPage(Uri xapUri, string htmlPageFullPath)
        {
            if (File.Exists(htmlPageFullPath))
            {
                File.Delete(htmlPageFullPath);
            }

            var content = _genericResourcesProvider.GetResourceContent("Silverlight4HostPage.htm");
            content = content.Replace("{XAP_URL}", xapUri.ToString());

            using (var stream = new FileStream(htmlPageFullPath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }

            return GetAbsoluteUrlForLocalFile(htmlPageFullPath);
        }

        private static Uri GetAbsoluteUrlForLocalFile(string path)
        {
            var fileUri = new Uri(path, UriKind.RelativeOrAbsolute);

            if (fileUri.IsAbsoluteUri)
            {
                return fileUri;
            }

            var baseUri = new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);

            return new Uri(baseUri, fileUri);
        }
    }
}