namespace CommonDeliveryFramework.Database
{

    /// <summary>
    /// Configuration data class used to hold the connection string for database connection.
    /// </summary>
    /// <typeparam name="T">The target type of the object that will use the connection string.</typeparam>
    public class DbConnectionString<T> where T:class
    {
        /// <summary>
        /// Backing field for the ConnectionStringProperty.
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="connectionString">Connection string to make available.</param>
        public DbConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// The database connection string.
        /// </summary>
        public string ConnectionString => _connectionString;
    }
}