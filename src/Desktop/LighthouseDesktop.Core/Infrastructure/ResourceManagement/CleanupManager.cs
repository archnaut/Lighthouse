using System;
using System.Collections.Generic;
using System.IO;
using LighthouseDesktop.Core.Infrastructure.Logging;

namespace LighthouseDesktop.Core.Infrastructure.ResourceManagement
{
    public interface ICleanupManager
    {
        IList<string> FilesToDelete { get; set; }
        IList<string> DirectoriesToDelete { get; set; }
        void Cleanup();
    }

    public class CleanupManager : ICleanupManager
    {
        private readonly ILogger _logger;

        public CleanupManager(ILogger logger)
        {
            _logger = logger;
        }

        private IList<string> _filesToDelete = new List<string>();
        public IList<string> FilesToDelete
        {
            get { return _filesToDelete; }
            set { _filesToDelete = value; }
        }

        private IList<string> _directoriesToDelete = new List<string>();
        public IList<string> DirectoriesToDelete
        {
            get { return _directoriesToDelete; }
            set { _directoriesToDelete = value; }
        }

        public void Cleanup()
        {
            foreach (var dir in DirectoriesToDelete)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch(Exception e)
                    {
                        _logger.Log(string.Format("Could not delete dir: {0} because: {1}", dir, e.Message));
                    }
                }
            }

            foreach (var file in FilesToDelete)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }
}