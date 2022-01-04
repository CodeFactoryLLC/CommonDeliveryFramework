using System;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Notifies that a communications exception has occurred and application safe message has been provided with the exception.
    /// </summary>
    public class CommunicationException : ManagedException
    {
        /// <summary>
        /// Creates an instance of <see cref="CommunicationException"/> and returns the default exception message.
        /// </summary>
        public CommunicationException() : base(StandardExceptionMessages.CommunicationException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="CommunicationException"/> that returns the default exception message and an imbedded exception.
        /// </summary>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public CommunicationException(Exception internalException) : base(StandardExceptionMessages.CommunicationException, internalException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="CommunicationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        public CommunicationException(string message) : base(message)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="CommunicationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public CommunicationException(string message, Exception internalException) : base(message, internalException)
        {
            //Intentionally blank
        }
    }
}
