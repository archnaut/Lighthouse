using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface IXapBuilder
    {
        string FullXapPath { get; set; }
        void AddFileToXap(string fileName, Stream assemblyStream);
        void AddFileToXapIfNotAlreadyThere(string fileName, Stream assemblyStream);
        void AddFileToXap(string fileName, string content);
        void AddFileToXap(string fileName, byte[] fileBytes);
        bool FileIsAlreadyInXap(string fileName);
        bool Save();
        void Clear();
    }

    public class XapBuilder : IXapBuilder, IDisposable
    {
        private ZipFile _zipFile;

        private List<string> _listOfFilesAdded = new List<string>();

        public string FullXapPath { get; set; }

        private void InitializeZipFileIfNeeded()
        {
            if (_zipFile == null)
            {
                _zipFile = new ZipFile();
                _listOfFilesAdded = new List<string>();
            }
        }

        public void Clear()
        {
            _zipFile = new ZipFile();
            _listOfFilesAdded = new List<string>();
        }

        public void AddFileToXapIfNotAlreadyThere(string fileName, Stream assemblyStream)
        {
            if (!FileIsAlreadyInXap(fileName))
            {
                AddFileToXap(fileName, assemblyStream);
            }
        }

        public void AddFileToXap(string fileName, Stream assemblyStream)
        {
            InitializeZipFileIfNeeded();
            _zipFile.AddEntry(fileName, assemblyStream);
            _listOfFilesAdded.Add(fileName.ToLower());
        }

        public void AddFileToXap(string fileName, string content)
        {
            InitializeZipFileIfNeeded();
            _zipFile.AddEntry(fileName, content);
            _listOfFilesAdded.Add(fileName.ToLower());
        }

        public bool FileIsAlreadyInXap(string fileName)
        {
            return _listOfFilesAdded.Contains(fileName.ToLower());
        }

        public bool Save()
        {
            if (_zipFile == null)
            {
                return false;
            }

            _zipFile.Save(FullXapPath);

            return true;
        }

        public void AddFileToXap(string fileName, byte[] fileBytes)
        {
            InitializeZipFileIfNeeded();
            _zipFile.AddEntry(fileName, fileBytes);            
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