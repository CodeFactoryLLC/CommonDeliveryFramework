using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDeliveryFramework.Net.Automation.Delivery
{
    /// <summary>
    /// Delivery information constants used through the automated delivery process.
    /// </summary>
    internal static class DeliveryInfo
    {
        /// <summary>
        /// Suffix for models that store sql data.
        /// </summary>
        public const string SqlModelLibrarySuffix = ".Model.Sql";

        /// <summary>
        /// Suffix for application models.
        /// </summary>
        public const string AppModelLibrarySuffix = ".Model.App";


        /// <summary>
        /// Suffix for models that are used with rest services.
        /// </summary>
        public const string RestModelLibrarySuffix = ".Model.Rest";

        /// <summary>
        /// Suffix for models that are used with Grpc services.
        /// </summary>
        public const string GrpcModelLibrarySuffix = ".Model.Rpc";

        /// <summary>
        /// Suffix for services that implement rest.
        /// </summary>
        public const string RestServiceLibrarySuffix = ".Service.Rest";

        /// <summary>
        /// Suffix for services that implement Grpc.
        /// </summary>
        public const string GrpcServiceLibrarySuffix = ".Service.Rpc";

        /// <summary>
        /// Suffix for application logic.
        /// </summary>
        public const string LogicLibrarySuffix = ".Logic";

        /// <summary>
        /// Suffix for abstractions that calls services.
        /// </summary>
        public const string AbstractionLibrarySuffix = ".Abstract";

        /// <summary>
        /// Suffix for the repository contracts used by repository implementations.
        /// </summary>
        public const string DataContractLibrarySuffix = ".Data.Contract";


        /// <summary>
        /// Suffix for repositories that support sql server.
        /// </summary>
        public const string SqlDataRepositorySuffix = ".Data.Sql";

        /// <summary>
        /// Suffix name used for repository classes.
        /// </summary>
        public const string RepositorySuffix = "Repository";

        /// <summary>
        /// Suffix for abstraction classes
        /// </summary>
        public const string AbstractionSuffix = "Abstract";

    }
}
