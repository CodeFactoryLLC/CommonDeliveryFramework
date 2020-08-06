using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Notifies that access is denied when communicating to an external resource and application safe message has been provided with the exception.
    /// </summary>
    public class ExternalAccessException : SecurityException
    {
        /// <summary>
        /// Creates an instance of <see cref="ExternalAccessException"/> and returns the default exception message.
        /// </summary>
        public ExternalAccessException() : base(StandardExceptionMessages.ExternalAccessException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="ExternalAccessException"/> that returns the default exception message and an imbedded exception.
        /// </summary>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public ExternalAccessException(Exception internalException) : base(StandardExceptionMessages.ExternalAccessException, internalException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="ExternalAccessException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        public ExternalAccessException(string message) : base(message)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="ExternalAccessException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public ExternalAccessException(string message, Exception internalException) : base(message, internalException)
        {
            //Intentionally blank
        }
    }
}
