using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface IXapReader
    {
        IList<string> Files { get;}
        void Load(string xapFileFullPath);
        string GetManifest();
        byte[] GetFileBytes(string fileName);
    }

    public class XapReader : IDisposable, IXapReader
    {
        private ZipFile _zipFile;
        private string _xapFullPath;

        private IList<string> _files = new List<string>();
        public IList<string> Files
        {
            get { return _files; }
            private set { _files = value; }
        }

        public void Load(string xapFileFullPath)
        {
            _xapFullPath = xapFileFullPath;
            _zipFile = new ZipFile(xapFileFullPath);  
            InitializeFilenames();
        }

        private void EnsureXapFileIsOpened()
        {
            if (_zipFile == null)
            {
                throw new Exception("First load XAP to get manifest.");
            }            
        }

        public string GetManifest()
        {
            EnsureXapFileIsOpened();

            var sourceManifest = _zipFile.SelectEntries("name = AppManifest.xaml").FirstOrDefault();
            if (sourceManifest == null)
            {
                throw new Exception(string.Format("Source xap {0} does not have an AppManifest.xaml", _xapFullPath));
            }

            using (var manifestStream = new MemoryStream())
            {
                sourceManifest.Extract(manifestStream);

                if (!manifestStream.CanRead)
                {
                    throw new Exception("Cannot read manifest stream");
                }

                manifestStream.Seek(0, SeekOrigin.Begin);
                var mainfestStreamReader = new StreamReader(manifestStream);
                var manifestContent = mainfestStreamReader.ReadToEnd();

                return manifestContent;
            }
        }

        public byte[] GetFileBytes(string fileName)
        {
            EnsureXapFileIsOpened();

            if (!Files.Contains(fileName))
            {
                throw new Exception(string.Format("File {0} does not exist in Xap {1}.", fileName, _xapFullPath));
            }

            using (var memoryStream = new MemoryStream())
            {
                var entry = _zipFile.Entries.Where(e => e.FileName == fileName).FirstOrDefault();
                entry.Extract(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }

        private void InitializeFilenames()
        {
            EnsureXapFileIsOpened();

            var items = _zipFile.SelectEntries("name = *.*");

            Files = new List<string>(items.Select(zipEntry => zipEntry.FileName));
        }

        public void Dispose()
        {
            if (_zipFile != null)
            {
                _zipFile.Dispose();
            }
        }
    }
}