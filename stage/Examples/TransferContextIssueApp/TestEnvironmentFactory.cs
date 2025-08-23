using Microsoft.Extensions.Logging;
using System.Threading;

namespace TransferContextIssueApp
{
    /// <summary>
    /// Factory class to create a test environment for USB device communication.
    /// </summary>
    internal static class TestEnvironmentFactory
    {
        /// <summary>
        /// Creates a test environment with the specified log level and cancellation token.
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public static ITestEnvironment Create(LogLevel logLevel, CancellationToken token)
        {
            // TODO Add your own implementation here
            return new DefaultTestEnvironment(logLevel, token);
        }
    }
}
