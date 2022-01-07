using ProtoBuf;

namespace CommonDeliveryFramework.Grpc;

/// <summary>
/// Grpc implementation of a exception that supports the base exception type of <see cref="ManagedException"/>
/// </summary>
[ProtoContract]
public class ManagedExceptionRpc
{
    /// <summary>
    /// Type of exception that occurred.
    /// </summary>
    [ProtoMember(1)]
    public string ExceptionType { get; set; }

    /// <summary>
    /// Message that occurred in the exception.
    /// </summary>
    [ProtoMember(2)]
    public string Message { get; set; }

    /// <summary>
    /// Which data field was impacted by the exception.
    /// </summary>
    [ProtoMember(3)]
    public string DataField { get; set; }

    /// <summary>
    /// Creates a new instance of a <see cref="ManagedExceptionRpc"/> from exceptions that are based on <see cref="ManagedException"/>
    /// </summary>
    /// <param name="exception">Exception to convert to a Grpc message.</param>
    /// <returns>Formatted Grpc message or null if not found.</returns>
    public static ManagedExceptionRpc CreateManagedExceptionRpc(ManagedException exception)
    {
        if (exception == null) return null;

        ManagedExceptionRpc result = null;

        result = exception switch
        {
            AuthenticationException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(AuthenticationException) },
            AuthorizationException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(AuthorizationException) },
            SecurityException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(SecurityException) },
            TimeoutException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(TimeoutException) },
            CommunicationException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(CommunicationException) },
            ConfigurationException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(ConfigurationException) },
            ValidationException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(ValidationException), DataField = exceptionData.DataField },
            DataException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(DataException) },
            DuplicateException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(DuplicateException) },
            LogicException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(LogicException) },
            UnhandledException exceptionData => new ManagedExceptionRpc { Message = exceptionData.Message, ExceptionType = nameof(UnhandledException) },
            _ => new ManagedExceptionRpc { Message = exception.Message, ExceptionType = nameof(ManagedException) },
        };
        return result;

    }

    /// <summary>
    /// Creates a <see cref="ManagedException"/> from the data.
    /// </summary>
    /// <returns>The target managed exception type.</returns>
    public ManagedException CreateManagedException()
    {
        ManagedException result = null;
        result = ExceptionType switch
        {
            nameof(AuthenticationException) => new AuthenticationException(Message),
            nameof(AuthorizationException) => new AuthorizationException(Message),
            nameof(SecurityException) => new SecurityException(Message),
            nameof(TimeoutException) => new TimeoutException(Message),
            nameof(CommunicationException) => new CommunicationException(Message),
            nameof(ConfigurationException) => new ConfigurationException(Message),
            nameof(ValidationException) => new ValidationException(Message, DataField),
            nameof(DataException) => new DataException(Message),
            nameof(DuplicateException) => new DuplicateException(Message),
            nameof(LogicException) => new LogicException(Message),
            nameof(UnhandledException) => new UnhandledException(Message),
            _ => new ManagedException(Message),
        };
        return result;
    }
}