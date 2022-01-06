namespace CommonDeliveryFramework.Service
{
    /// <summary>
    /// Base implementation for a service url definition.
    /// </summary>
    public abstract class ServiceUrl
    {
        /// <summary>
        /// backing field for the url path.
        /// </summary>
        private readonly string _url;

        /// <summary>
        /// Creates a instance of the service url.
        /// </summary>
        /// <param name="url">URL of the service.</param>
        protected ServiceUrl(string url)
        {
            _url = url;
        }

        /// <summary>
        /// The url where a service is hosted.
        /// </summary>
        public string Url => _url;
    }
}