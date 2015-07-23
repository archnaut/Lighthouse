using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public class Silverlight4AppManifestAnalyzer : ISilverlightApplicationManifestAnalyzer
    {
        private readonly XNamespace _xamlNamespace;
        private static XName _nameAttributeName;
        
        public Silverlight4AppManifestAnalyzer()
        {
            _xamlNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
            _nameAttributeName = _xamlNamespace.GetName("Name");
        }

        public SilverlightManifestAnalysisResult Analyze(string manifestContent)
        {
            if (string.IsNullOrEmpty(manifestContent))
            {
                throw new Exception("Empty manifest");
            }

            return ParseManifestContent(manifestContent);
        }

        private SilverlightManifestAnalysisResult ParseManifestContent(string manifestContent)
        {
            var result = new SilverlightManifestAnalysisResult();

            var manifest = XDocument.Parse(manifestContent);
            if (manifest.Root == null)
            {
                throw new Exception("Manifest does not have Root element");
            }

            var entryPointAssemblyAttribute = manifest.Root.Attribute("EntryPointAssembly");
            result.EntryPointAssemblyName = entryPointAssemblyAttribute != null ? entryPointAssemblyAttribute.Value : null;

            var entryPointTypeAttribute = manifest.Root.Attribute("EntryPointType");
            result.EntryPointTypeName = entryPointTypeAttribute != null ? entryPointTypeAttribute.Value : null;

            var manifestItems = manifest.Root.Descendants().First().Descendants();
            result.AssemblyPartItems.Clear();

            foreach (var manifestItem in manifestItems)
            {
                var name = manifestItem.Attribute(_nameAttributeName);
                var source = manifestItem.Attribute("Source");

                if (name != null &&  source != null)
                {
                    var nameValue = name.Value;
                    var sourceValue = source.Value;

                    if (!string.IsNullOrEmpty(nameValue) && !string.IsNullOrEmpty(sourceValue))
                    {
                        var currentAssemblyPart = new ManifestAssemblyPartItem()
                                                  {
                                                      Source = sourceValue,
                                                      Name = nameValue
                                                  };

                        result.AssemblyPartItems.Add(currentAssemblyPart);

                        if (!string.IsNullOrEmpty(result.EntryPointAssemblyName))
                        {
                            if (currentAssemblyPart.Name == result.EntryPointAssemblyName)
                            {
                                result.EntryPointAssemblyDllName = currentAssemblyPart.Source;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public SilverlightManifestAnalysisResult Analyze(Stream manifestStream)
        {
            if (manifestStream == null || !manifestStream.CanRead )
            {
                throw new Exception("Cannot read manifest stream");
            }

            manifestStream.Seek(0, SeekOrigin.Begin);
            var mainfestStreamReader = new StreamReader(manifestStream);
            var manifestContent = mainfestStreamReader.ReadToEnd();

            return Analyze(manifestContent);
        }
    }
}