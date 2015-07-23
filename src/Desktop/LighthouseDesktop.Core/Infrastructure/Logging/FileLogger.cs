using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LighthouseDesktop.Core.Infrastructure.Logging
{
    public interface IFileLogger : ILogger
    {
        bool IsActive { get; set; }

        string LogFileName { get; set; }
    }

    public class FileLogger : IFileLogger
    {
        private static object _globalLocker = new object();

        public bool IsActive { get; set; }

        public string LogFileName { get; set; }

        public void Log(string message)
        {
            if (IsActive && FilenameIsDefined())
            {
                lock (GetLockForFilename(LogFileName))
                {
                    using (var stream = new FileStream(LogFileName, FileMode.Append))
                    {
                        var bytes = Encoding.UTF8.GetBytes(string.Format("{0}\n", message));
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Close();
                    }
                }
            }
        }

        private static Dictionary<string, object> _filenameLocks = new Dictionary<string,object>();

        private static object GetLockForFilename(string logFileName)
        {
            lock (_globalLocker)
            {
                if (!_filenameLocks.ContainsKey(logFileName))
                {
                    _filenameLocks.Add(logFileName, new object());
                }

                return _filenameLocks[logFileName];
            }
        }

        private bool FilenameIsDefined()
        {
            return !string.IsNullOrEmpty(LogFileName);
        }
    }
}