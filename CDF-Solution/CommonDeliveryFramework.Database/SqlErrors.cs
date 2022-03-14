using Microsoft.Data.SqlClient;

namespace CommonDeliveryFramework.Database
{
    /// <summary>
    /// Error levels that tracks the type of SQL error that has occurred.
    /// </summary>
    public static class SqlErrors
    {
        /// <summary>
        /// Data fails due to a duplicate key or validation constraint.
        /// </summary>
        public const int DuplicateEntry = 2601;

        /// <summary>
        /// Data fails to a unique key constraint.
        /// </summary>
        public const int UniqueKeyConstraintViolation = 2627;

        public const string DuplicateEntryMessage =
            "A duplicate copy of the data was found in storage, cannot have duplicates.";

        /// <summary>
        /// Connection failure to the SQL Environment.
        /// </summary>
        public const int Connection = 10060;

        public const string ConnectionMessage = "Internal error occurred while accessing the data.";

        /// <summary>
        /// Timeout has occurred completing the operation.
        /// </summary>
        public const int Timeout = -2;

        public const string TimeoutMessage = "The system timed out accessing the data.";

        /// <summary>
        /// Authentication failure
        /// </summary>
        public const int Authentication = 18456;

        public const string AuthenticationMessage = "Was note able to access the data source due to an internal error";

        /// <summary>
        /// Creates a managed exception for the sql exception that occurred.
        /// </summary>
        /// <param name="source">Source sql exception to review and throw.</param>
        public static void GenerateManagedException(this SqlException source)
        {
            if (source == null) return;

            switch (source.Number)
            {
                case SqlErrors.Authentication:

                    throw new AuthenticationException(AuthenticationMessage);
                    break;

                case SqlErrors.Connection:

                    throw new CommunicationException(ConnectionMessage);
                    break;

                case SqlErrors.DuplicateEntry:
                case SqlErrors.UniqueKeyConstraintViolation:
                    throw new DuplicateException(DuplicateEntryMessage);
                    break;

                case SqlErrors.Timeout:

                    throw new TimeoutException(TimeoutMessage);
                    break;

                default:

                    throw new DataException();
                    break;
            }
        }

    }
}