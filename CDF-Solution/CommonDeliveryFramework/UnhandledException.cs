using System;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Exception that is raised an an application exception has occured and there was no catch logic to handle the exception. This returns a safe error messages to consumers of the exception.
    /// </summary>
    public class UnhandledException : ManagedException
    {
        /// <summary>
        /// Creates an instance of <see cref="UnhandledException"/> and returns the default exception message.
        /// </summary>
        public UnhandledException() : base(StandardExceptionMessages.UnhandledException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="UnhandledException"/> that returns the default exception message and an imbedded exception.
        /// </summary>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public UnhandledException(Exception internalException) : base(StandardExceptionMessages.UnhandledException, internalException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="UnhandledException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        public UnhandledException(string message) : base(message)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="UnhandledException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public UnhandledException(string message, Exception internalException) : base(message, internalException)
        {
            //Intentionally blank
        }
    }
}
