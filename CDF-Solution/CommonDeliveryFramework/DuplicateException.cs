using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Managed exception that provides notification when data is being duplicated in the system.
    /// </summary>
    public class DuplicateException:ManagedException
    {
        /// <summary>
        /// Creates an instance of <see cref="DuplicateException"/> and returns the default exception message.
        /// </summary>
        public DuplicateException() : base(StandardExceptionMessages.DuplicateException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="DuplicateException"/> that returns the default exception message and an embedded exception.
        /// </summary>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public DuplicateException(Exception internalException) : base(StandardExceptionMessages.DuplicateException, internalException)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="DuplicateException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        public DuplicateException(string message) : base(message)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Creates an instance of <see cref="DuplicateException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        public DuplicateException(string message, Exception internalException) : base(message, internalException)
        {
            //Intentionally blank
        }
    }
}
