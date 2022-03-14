using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDeliveryFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Service.Rpc
{
    public class Loader:DependencyInjectionLibraryLoader
    {
        /// <inheritdoc />
        protected override void LoadLibraries(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var sqlLoader = new Demo.Data.Sql.Loader();
            sqlLoader.Load(serviceCollection,configuration);

            var logicLoader = new Demo.Logic.Loader();
            logicLoader.Load(serviceCollection,configuration);
        }

        protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            
        }


        /// <inheritdoc />
        protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {

        }
    }
}
