using Microsoft.AspNetCore.Mvc;

namespace CommonDeliveryFramework.Net.Aspnet
{
    /// <summary>
    /// Extensions class that extends the ASP.Net core <see cref="ControllerBase"/> class.
    /// These extensions roll up integration of utilities library functionality and makes them available to web api controllers.
    /// </summary>
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// Extension method that generates a problem result from an exception that ultimately inherits from <see cref="ManagedException"/>
        /// </summary>
        /// <param name="source">The source controller to generate the problem for.</param>
        /// <param name="exception">The target exception to be generated as a problem.</param>
        /// <returns>The formatted problem ready to be raised by the controller.</returns>
        public static ProblemObjectResult CreateProblemResult(this ControllerBase source, ManagedException exception)
        {
            if (exception == null | source == null)
            {

                return CreateGenericInternalServerErrorProblemResult();
            }

            ProblemDetails problem = null;

            switch (exception)
            {
                
                case ExternalAccessException accessDeniedException:
                    problem = accessDeniedException.GetProblemDetails(source);
                    break;
                case SecurityException securityException:
                    problem = securityException.GetProblemDetails(source);
                    break;
                case TimeoutException timeoutException:
                    problem = timeoutException.GetProblemDetails(source);
                    break;
                case CommunicationException communicationsException:
                    problem = communicationsException.GetProblemDetails(source);
                    break;
                case ConfigurationException configurationException:
                    problem = configurationException.GetProblemDetails(source);
                    break;
                case ValidationException dataValidationException:
                    problem = dataValidationException.GetProblemDetails(source);
                    break;
                case DataException dataException:
                    problem = dataException.GetProblemDetails(source);
                    break;
                case UnhandledException unhandledLogicException:
                    problem = unhandledLogicException.GetProblemDetails(source);
                    break;
                case LogicException logicException:
                    problem = logicException.GetProblemDetails(source);
                    break;
                default:
                    problem = exception.GetProblemDetails(source);
                    break;
            }

            return problem != null ? new ProblemObjectResult(problem) : CreateGenericInternalServerErrorProblemResult();
        }

        /// <summary>
        /// Static helper method to create a generic internal server error problem.
        /// </summary>
        /// <returns>Generic internal server error problem as a action result.</returns>
        private static ProblemObjectResult CreateGenericInternalServerErrorProblemResult()
        {
            var problem = new ProblemDetails
            { Type = "https://httpstatuses.com/500", Status = 500, Title = "Internal Server Error", Detail = "An internal error has occurred and the operation could not complete." };

            return new ProblemObjectResult(problem);
        }
    }
}
