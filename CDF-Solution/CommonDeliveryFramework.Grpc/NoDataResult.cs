using ProtoBuf;

namespace CommonDeliveryFramework.Grpc
{
    /// <summary>
    /// Result from a service call with no data being returned.
    /// </summary>
    [ProtoContract]
    public class NoDataResult
    {
        /// <summary>
        /// Boolean flag that determines if the service result had and exception occur.
        /// </summary>
        [ProtoMember(1)]
        public bool HasExceptions { get; set; }

        /// <summary>
        /// If <see cref="HasExceptions"/> is true then the ManagedException will be provided.
        /// </summary>
        [ProtoMember(2)]
        public List<ManagedExceptionRpc> ManagedExceptions { get; set; }


        /// <summary>
        /// Returns a new instance of <see cref="NoDataResult"/> with a single exception.
        /// </summary>
        /// <param name="exception">Exception to return.</param>
        /// <returns>Service result with error.</returns>
        public static NoDataResult CreateError(ManagedException exception)
        {
            return new NoDataResult
            {
                HasExceptions = true,
                ManagedExceptions = new List<ManagedExceptionRpc>
                    {ManagedExceptionRpc.CreateManagedExceptionRpc(exception)}
            };
        }

        /// <summary>
        /// Returns a new instance of <see cref="NoDataResult"/> with multiple exceptions.
        /// </summary>
        /// <param name="exceptions">Exceptions to return.</param>
        /// <returns>Result with errors.</returns>
        public static NoDataResult CreateErrors(List<ManagedException> exceptions)
        {
            return new NoDataResult
            {
                HasExceptions = true,
                ManagedExceptions = exceptions.Select(ManagedExceptionRpc.CreateManagedExceptionRpc).ToList()
            };
        }

        /// <summary>
        /// Returns a new instance of <see cref="NoDataResult"/> as a successful operation.
        /// </summary>
        public static NoDataResult CreateSuccess()
        {
            return new NoDataResult {HasExceptions = false};
        }

        /// <summary>
        /// Helper method that will raise the first captured managed exception.
        /// </summary>
        public void RaiseException()
        {
            if (!HasExceptions) return;

            var managedException = ManagedExceptions.FirstOrDefault();

            if (managedException != null) throw managedException.CreateManagedException();

        }
    }
}
