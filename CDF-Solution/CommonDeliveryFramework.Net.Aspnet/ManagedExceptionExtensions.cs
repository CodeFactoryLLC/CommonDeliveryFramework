using Microsoft.AspNetCore.Mvc;

namespace CommonDeliveryFramework.Net.Aspnet
{
    /// <summary>
    /// Extension methods that are based on the exceptions that inherit from  <see cref="ManagedException"/>.
    /// </summary>
    public static class ManagedExceptionExtensions
    {
        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/403 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 403.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Forbidden.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this ExternalAccessException source,
            ControllerBase controller, string type = "https://httpstatuses.com/403",
            int? status = 403, string title = "Forbidden")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/504 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 504.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Gateway Timeout.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this CommunicationException source,
            ControllerBase controller, string type = "https://httpstatuses.com/504",
            int? status = 504, string title = "Gateway Timeout")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/500 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 500.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Internal Server Error.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this ConfigurationException source,
            ControllerBase controller, string type = "https://httpstatuses.com/500",
            int? status = 500, string title = "Internal Server Error")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/500 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 500.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Internal Server Error.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this DataException source,
            ControllerBase controller, string type = "https://httpstatuses.com/500",
            int? status = 500, string title = "Internal Server Error")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/400 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 400.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Bad Request.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this ValidationException source,
            ControllerBase controller, string type = "https://httpstatuses.com/400",
            int? status = 400, string title = "Bad Request")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/500 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 500.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Internal Server Error.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this LogicException source,
            ControllerBase controller, string type = "https://httpstatuses.com/500",
            int? status = 500, string title = "Internal Server Error")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/500 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 500.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Internal Server Error.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this ManagedException source,
            ControllerBase controller, string type = "https://httpstatuses.com/500",
            int? status = 500, string title = "Internal Server Error")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/401 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 401.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to UnAuthorized.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this SecurityException source,
            ControllerBase controller, string type = "https://httpstatuses.com/401",
            int? status = 401, string title = "UnAuthorized")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/504 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 504.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to GatewayTimeout.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this TimeoutException source,
            ControllerBase controller, string type = "https://httpstatuses.com/504",
            int? status = 504, string title = "Gateway Timeout")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }

        /// <summary>
        /// Extension method that generates problem details for the exception.
        /// </summary>
        /// <param name="source">The exception to map to a <see cref="ProblemDetails"/></param>
        /// <param name="controller">The base controller the exception occurred in. </param>
        /// <param name="type">Optional parameter that sets the URL for the human readable explanation for the status code. Default value is set to https://httpstatuses.com/500 </param>
        /// <param name="status">Optional parameter that sets the return status code for the problem. Default value is set to 500.</param>
        /// <param name="title">Optional parameter that sets the title for the problem. Default value is set to Internal Server Error.</param>
        /// <returns>Instance of the problem details.</returns>
        public static ProblemDetails GetProblemDetails(this UnhandledException source,
            ControllerBase controller, string type = "https://httpstatuses.com/500",
            int? status = 500, string title = "Internal Server Error")
        {
            return new ProblemDetails { Type = type, Status = status, Title = title, Instance = controller?.HttpContext?.Request?.Path, Detail = source?.Message };
        }


    }
}
