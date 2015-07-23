using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Lighthouse.Common.Interoperability;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;

namespace Lighthouse.Common.Services
{
    public interface ISerializationService
    {
        string Serialize<T>(T graph) where T : class;
        T Deserialize<T>(string data) where T : class;
    }

    public class SerializationService : ISerializationService
    {
        private IEnumerable<Type> _knownTypes;

        public string Serialize<T>(T graph) where T : class 
        {
            if (graph == null)
            {
                throw new Exception("Cannot serialize null.");
            }

            var serializer = new DataContractSerializer(typeof(T), KnownTypes);

            using (var writer = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(writer))
            {
                serializer.WriteObject(xmlWriter, graph);
                xmlWriter.Flush();
                writer.Flush();

                return writer.ToString();
            }
        }

        private IEnumerable<Type> KnownTypes
        {
            get
            {
                if (_knownTypes == null)
                {
                    _knownTypes = GetKnownTypes();
                }

                return _knownTypes;
            }

        }

        public T Deserialize<T>(string data) where T : class 
        {
            var serializer = new DataContractSerializer(typeof(T), KnownTypes);

            using (var reader = new StringReader(data))
            using (var xmlReader = XmlReader.Create(reader))
            {
                return (T)serializer.ReadObject(xmlReader);
            }
        }

        private IEnumerable<Type> GetKnownTypes()
        {
            var assembly = typeof(UnitTestOutcome).Assembly;
            var namespaces = new List<string>()
                                 {
                                     typeof (UnitTestOutcome).Namespace,
                                     typeof(SilverlightUnitTestRunSettings).Namespace
                                 };

            return assembly.GetTypes().Where(t => !t.IsInterface && namespaces.Contains(t.Namespace)).ToList();
        }
    }
}