using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LibUsbDotNet.ReadTransferQueueApp
{
    /// <summary>
    /// Represents a simple console logger factory.
    /// </summary>
    public sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        private readonly TextWriter _textWriter;
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLoggerFactory"/> class.
        /// </summary>
        public ConsoleLoggerFactory(LogLevel logLevel = LogLevel.Information)
        {
            _textWriter = Console.Out;
            _logLevel = logLevel;
        }

        /// <inheritdoc/>
        public void AddProvider(ILoggerProvider provider)
        {
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(_textWriter, categoryName, _logLevel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Represents a simple console logger.
        /// </summary>
        private sealed class ConsoleLogger : ILogger
        {
            private readonly TextWriter _textWriter;
            private readonly string _categoryName;
            private readonly LogLevel _logLevel;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
            /// </summary>
            /// <param name="textWriter">Text writer</param>
            /// <param name="categoryName">Category name</param>
            public ConsoleLogger(TextWriter textWriter, string categoryName, LogLevel logLevel)
            {
                _textWriter = textWriter;
                _categoryName = categoryName;
                _logLevel = logLevel;
            }

            /// <inheritdoc/>
            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            /// <inheritdoc/>
            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= _logLevel;
            }

            /// <inheritdoc/>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var message = formatter(state, exception);
                var logOutput = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{_categoryName}] {message}";

                _textWriter.WriteLine(logOutput);
            }
        }
    }
}
