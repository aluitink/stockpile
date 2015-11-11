using System.IO;
using log4net;
using Microsoft.Framework.Logging;

namespace Stockpile.Api.App
{
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private readonly FileInfo _configFile;
        private readonly string _logRootDirKey = "logrootdir";

        public Log4NetLoggerProvider(FileInfo configFile, DirectoryInfo logDirectory)
        {
            _configFile = configFile;
            InitializeLogDirectory(logDirectory);
        }

        public ILogger CreateLogger(string name)
        {
            return new Log4NetLogger(name, _configFile);
        }

        private void InitializeLogDirectory(DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();

            GlobalContext.Properties[_logRootDirKey] = directory.FullName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public void Dispose() { }
    }
}