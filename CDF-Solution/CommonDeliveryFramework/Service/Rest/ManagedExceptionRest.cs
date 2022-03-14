
namespace CommonDeliveryFramework.Service.Rest
{
    /// <summary>
    /// Rest implementation of a exception that supports the base exception type of <see cref="ManagedException"/>
    /// </summary>
    public class ManagedExceptionRest
    {
        /// <summary>
        /// Type of exception that occurred.
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Message that occurred in the exception.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Which data field was impacted by the exception.
        /// </summary>
        public string DataField { get; set; }

        /// <summary>
        /// Creates a new instance of a <see cref="ManagedExceptionRest"/> from exceptions that are based on <see cref="ManagedException"/>
        /// </summary>
        /// <param name="exception">Exception to convert to a Grpc message.</param>
        /// <returns>Formatted Grpc message or null if not found.</returns>
        public static ManagedExceptionRest CreateManagedExceptionRest(ManagedException exception)
        {
            if (exception == null) return null;

            ManagedExceptionRest result = null;

            switch (exception)
            {
                case AuthenticationException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(AuthenticationException)
                    };
                    break;
                case AuthorizationException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(AuthorizationException)
                    };
                    break;
                case SecurityException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(SecurityException)
                    };
                    break;
                case TimeoutException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(TimeoutException)
                    };
                    break;
                case CommunicationException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(CommunicationException)
                    };
                    break;
                case ConfigurationException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(ConfigurationException)
                    };
                    break;
                case ValidationException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message,
                        ExceptionType = nameof(ValidationException),
                        DataField = exceptionData.DataField
                    };
                    break;
                case DataException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(DataException)
                    };
                    break;
                case DuplicateException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(DuplicateException)
                    };
                    break;
                case LogicException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(LogicException)
                    };
                    break;
                case UnhandledException exceptionData:
                    result = new ManagedExceptionRest
                    {
                        Message = exceptionData.Message, ExceptionType = nameof(UnhandledException)
                    };
                    break;
                default:
                    result = new ManagedExceptionRest
                    {
                        Message = exception.Message, ExceptionType = nameof(ManagedException)
                    };
                    break;
            }

            return result;

        }

        /// <summary>
        /// Creates a <see cref="ManagedException"/> from the data.
        /// </summary>
        /// <returns>The target managed exception type.</returns>
        public ManagedException CreateManagedException()
        {
            ManagedException result = null;
            switch (ExceptionType)
            {
                case nameof(AuthenticationException):
                    result = new AuthenticationException(Message);
                    break;
                case nameof(AuthorizationException):
                    result = new AuthorizationException(Message);
                    break;
                case nameof(SecurityException):
                    result = new SecurityException(Message);
                    break;
                case nameof(TimeoutException):
                    result = new TimeoutException(Message);
                    break;
                case nameof(CommunicationException):
                    result = new CommunicationException(Message);
                    break;
                case nameof(ConfigurationException):
                    result = new ConfigurationException(Message);
                    break;
                case nameof(ValidationException):
                    result = new ValidationException(Message, DataField);
                    break;
                case nameof(DataException):
                    result = new DataException(Message);
                    break;
                case nameof(DuplicateException):
                    result = new DuplicateException(Message);
                    break;
                case nameof(LogicException):
                    result = new LogicException(Message);
                    break;
                case nameof(UnhandledException):
                    result = new UnhandledException(Message);
                    break;
                default:
                    result = new ManagedException(Message);
                    break;
            }

            return result;
        }
    }
}
