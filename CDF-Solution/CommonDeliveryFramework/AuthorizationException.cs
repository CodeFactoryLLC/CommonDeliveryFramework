using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Notifies that a authorization security exception has been captured and a application safe message has been added to this exception.
    /// </summary>
    public class AuthorizationException:SecurityException
    {
        /// <summary>
        /// Creates an instance of <see cref="AuthorizationException"/> and returns the default exception message.
        /// </summary>
        public AuthorizationException() : base(StandardExceptionMessages.AuthorizationException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationException"/> that returns the default exception message and an embedded exception.
        /// </summary>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public AuthorizationException(Exception internalException) : base(StandardExceptionMessages.AuthorizationException, internalException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        public AuthorizationException(string message) : base(message)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public AuthorizationException(string message, Exception internalException) : base(message, internalException)
        {
            //Intentionally blank
        }
    }
}
