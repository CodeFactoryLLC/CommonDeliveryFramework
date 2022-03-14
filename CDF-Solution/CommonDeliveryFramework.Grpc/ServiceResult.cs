using ProtoBuf;

namespace CommonDeliveryFramework.Grpc;

/// <summary>
/// Result data from a service call.
/// </summary>
/// <typeparam name="T">The target type of data being returned from the service call.</typeparam>
[ProtoContract]
public class ServiceResult<T> where T:class
{
    /// <summary>
    /// Result data from the service call.
    /// </summary>
    [ProtoMember(1)]
    public T Result { get; set; }

    /// <summary>
    /// Boolean flag that determines if the service result had and exception occur.
    /// </summary>
    [ProtoMember(2)]
    public bool HasExceptions { get; set; }

    /// <summary>
    /// If <see cref="HasExceptions"/> is true then the ManagedException will be provided.
    /// </summary>
    [ProtoMember(3)]
    public List<ManagedExceptionRpc> ManagedExceptions { get; set; }

    /// <summary>
    /// Returns a new instance of a service result with a single exception.
    /// </summary>
    /// <param name="exception">Exception to return.</param>
    /// <returns>Service result with error.</returns>
    public static ServiceResult<T> CreateError(ManagedException exception)
    {
        return new ServiceResult<T>
        {
            HasExceptions = true,
            ManagedExceptions = new List<ManagedExceptionRpc>
                {ManagedExceptionRpc.CreateManagedExceptionRpc(exception)}
        };
    }

    /// <summary>
    /// Returns a new instance of a service result with multiple exceptions.
    /// </summary>
    /// <param name="exceptions">Exceptions to return.</param>
    /// <returns>Service result with error.</returns>
    public static ServiceResult<T> CreateErrors(List<ManagedException> exceptions)
    {
        return new ServiceResult<T>
        {
            HasExceptions = true,
            ManagedExceptions = exceptions.Select(e => ManagedExceptionRpc.CreateManagedExceptionRpc(e)).ToList()
        };
    }

    /// <summary>
    /// Returns a new instance of a service result with result data.
    /// </summary>
    /// <param name="result">result data to return.</param>
    /// <returns>Service result with error.</returns>
    public static ServiceResult<T> CreateResult(T result)
    {
        return new ServiceResult<T> {HasExceptions = false, Result = result};
    }

    public void RaiseException()
    {
        if (!HasExceptions) return;

        var managedException = ManagedExceptions.FirstOrDefault();

        if (managedException != null) throw managedException.CreateManagedException();

    }
}