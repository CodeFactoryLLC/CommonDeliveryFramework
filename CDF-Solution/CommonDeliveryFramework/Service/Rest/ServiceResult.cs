
using System.Collections.Generic;
using System.Linq;

namespace CommonDeliveryFramework.Service.Rest
{
    /// <summary>
    /// Result data from a service call.
    /// </summary>
    /// <typeparam name="T">The target type of data being returned from the service call.</typeparam>
    public class ServiceResult<T> 
    {
        /// <summary>
        /// Result data from the service call.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Boolean flag that determines if the service result had and exception occur.
        /// </summary>
        public bool HasExceptions { get; set; }

        /// <summary>
        /// If <see cref="HasExceptions"/> is true then the ManagedException will be provided.
        /// </summary>
        public List<ManagedExceptionRest> ManagedExceptions { get; set; }

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
                ManagedExceptions = new List<ManagedExceptionRest>
                    {ManagedExceptionRest.CreateManagedExceptionRest(exception)}
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
                ManagedExceptions = exceptions.Select(e => ManagedExceptionRest.CreateManagedExceptionRest(e)).ToList()
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
}
