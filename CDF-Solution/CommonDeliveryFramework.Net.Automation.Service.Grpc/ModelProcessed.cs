using CodeFactory.DotNet.CSharp;

namespace CommonDeliveryFramework.Net.Automation.Service.Grpc
{
    /// <summary>
    /// Immutable data class that determines if a model has already been processed. 
    /// </summary>
    public class ModelProcessed
    {
        private readonly CsClass _sourceClass;
        private readonly CsClass _targetClass;

        /// <summary>
        /// Creates an instance of <see cref="ModelProcessed"/>
        /// </summary>
        /// <param name="sourceClass">Source class that is used to build the target model.</param>
        /// <param name="targetClass">Target class that is the implemented target model. </param>
        public ModelProcessed(CsClass sourceClass, CsClass targetClass)
        {
            _sourceClass = sourceClass;
            _targetClass = targetClass;
        }

        /// <summary>
        /// Source class that is used to build the target model.
        /// </summary>
        public CsClass SourceClass => _sourceClass;

        /// <summary>
        /// Target class that is the implemented target model. 
        /// </summary>
        public CsClass TargetClass => _targetClass;

    }
}