using System;

namespace CommonDeliveryFramework
{
    /// <summary>
    /// Attribute that informs a model data class handles model conversion.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class ModelConvertAttribute:Attribute
    {
        /// <summary>
        /// Backing field for property <see cref="ModelType"/>
        /// </summary>
        private Type _modelType;

        public ModelConvertAttribute(Type modelType)
        {
            _modelType = modelType;
        }

        /// <summary>
        /// The source model that was used for conversion to this model class.
        /// </summary>
        public Type ModelType => _modelType;
    }
}