using System;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Notifies that an exception has occurred while validating data within the application an application safe message has been provided with the exception.
    /// </summary>
    public class ValidationException : ManagedException
    {
        /// <summary>
        /// The data field that has failed validation.
        /// </summary>
        public string DataField { get; }

        /// <summary>
        /// Creates an instance of <see cref="ValidationException"/> and returns the default exception message.
        /// </summary>
        /// <param name="dataField">Optional parameter that provides the name of the data field where validation failed.</param>
        public ValidationException(string dataField = null) : base(StandardExceptionMessages.ValidationException)
        {
            DataField = dataField;
        }

        /// <summary>
        /// Creates an instance of <see cref="ValidationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="dataField">Optional parameter that provides the name of the data field where validation failed.</param>
        public ValidationException(string message, string dataField = null) : base(message)
        {
            DataField = dataField;
        }

        /// <summary>
        /// Creates an instance of <see cref="ValidationException"/> exception class.
        /// </summary>
        /// <param name="message">Message to be returned as part of the exception.</param>
        /// <param name="internalException">Existing exception to be added to this exception.</param>
        /// <param name="dataField">Optional parameter that provides the name of the data field where validation failed.</param>
        public ValidationException(string message, Exception internalException, string dataField = null) : base(message, internalException)
        {
            DataField = dataField;
        }
    }
}
