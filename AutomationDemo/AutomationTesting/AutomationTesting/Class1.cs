
using System.Threading.Tasks;
using System;
using CommonDeliveryFramework;
using Microsoft.Extensions.Logging;
namespace AutomationTesting
{
    public class Class1:ITest
    {

		/// <summary>
		/// Logger for all logging interactions in the class.
		/// </summary>
		private readonly ILogger _logger;

        public Class1(ILogger<Class1> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetNameAsync(Guid id, string lastName, string firstName = null)
		{
			_logger.InformationEnterLog();
		
			if(string.IsNullOrEmpty(lastName))
			{
				_logger.ErrorLog($"The parameter {nameof(lastName)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(lastName));
			}
		
			try
			{
				//TODO: add execution logic here
			}
			catch (ManagedException)
			{
				//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.
				throw;
			}
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
		
			_logger.InformationExitLog();
			throw new NotImplementedException();
		
			//TODO: add return logic here
		}

		
		public string FirstName { get; set; }
				
    }
}