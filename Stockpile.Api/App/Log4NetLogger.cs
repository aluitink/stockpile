using System;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.Framework.Logging;
using ILogger = Microsoft.Framework.Logging.ILogger;

namespace Stockpile.Api.App
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        private static bool _isConfigured;
        public Log4NetLogger(string name, FileInfo configFile)
        {
            Configure(configFile);
            _log = LogManager.GetLogger(name);
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            var logVal = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Debug:
                    _log.Debug(logVal);
                    break;
                case LogLevel.Verbose:
                    _log.Verbose(logVal);
                    break;
                case LogLevel.Information:
                    _log.Info(logVal);
                    break;
                case LogLevel.Warning:
                    _log.Warn(logVal, exception);
                    break;
                case LogLevel.Error:
                    _log.Error(logVal, exception);
                    break;
                case LogLevel.Critical:
                    _log.Fatal(logVal, exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _log.Logger.IsEnabledFor(GetLog4NetLevel(logLevel));
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return null;
        }

        private Level GetLog4NetLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return Level.Debug;
                case LogLevel.Verbose:
                    return Level.Verbose;
                case LogLevel.Information:
                    return Level.Info;
                case LogLevel.Warning:
                    return Level.Warn;
                case LogLevel.Error:
                    return Level.Error;
                case LogLevel.Critical:
                    return Level.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        private static void Configure(FileInfo configFile)
        {
            if (_isConfigured) return;

            if (!configFile.Exists)
                throw new ApplicationException("Could not configure Logging");

            XmlConfigurator.ConfigureAndWatch(configFile);
            _isConfigured = true;
        }
    }
}
