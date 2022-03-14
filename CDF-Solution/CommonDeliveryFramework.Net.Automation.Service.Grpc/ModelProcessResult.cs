using System.Collections.Immutable;

namespace CommonDeliveryFramework.Net.Automation.Service.Grpc
{
    /// <summary>
    /// Data class that provides a result from a model process automation.
    /// </summary>
    /// <typeparam name="T">Result type for the operation.</typeparam>
    public class ModelProcessResult<T>
    {

        private readonly ImmutableList<ModelProcessed> _processedModels;

        private readonly T _result;

        /// <summary>
        /// Creates a new instance of the result data model.
        /// </summary>
        /// <param name="processedModels">Immutable list of the models that have been processed.</param>
        /// <param name="result">The result data from the model process operation.</param>
        public ModelProcessResult(ImmutableList<ModelProcessed> processedModels, T result)
        {
            _processedModels = processedModels;
            _result = result;
        }

        /// <summary>
        /// List of models that have been processed.
        /// </summary>
        public ImmutableList<ModelProcessed> ProcessedModel => _processedModels;

        /// <summary>
        /// Result from the model process operation.
        /// </summary>
        public T Result => _result;
    }
}