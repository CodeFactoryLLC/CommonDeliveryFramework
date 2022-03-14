using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDeliveryFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTesting
{
    public class Loader:DependencyInjectionLibraryLoader
    {
        /// <inheritdoc />
        protected override void LoadLibraries(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {

        }
    }
}
