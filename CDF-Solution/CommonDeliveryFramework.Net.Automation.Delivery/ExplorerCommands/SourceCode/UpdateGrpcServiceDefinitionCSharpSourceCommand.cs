using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CodeFactory;
using CommonDeliveryFramework.Net.Automation.Common;
using CommonDeliveryFramework.Net.Automation.Delivery.Logic;
using  CommonDeliveryFramework.Net.Automation.Service.Grpc;

namespace CommonDeliveryFramework.Net.Automation.Delivery.ExplorerCommands.SourceCode
{
/// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class UpdateGrpcServiceDefinitionCSharpSourceCommand : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Update Grpc Service Definition";
        private static readonly string commandDescription = "Updates a service and abstraction definition and logic that support Grpc.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public UpdateGrpcServiceDefinitionCSharpSourceCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                isEnabled = await result.IsLogicProjectAsync();
                if(isEnabled) isEnabled = result.SourceCode.Interfaces.Any();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while checking if the solution explorer C# document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsCSharpSource result)
        {
            try
            {
                var appModelProject = await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.AppModelLibrarySuffix);

                var rpcModelProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.GrpcModelLibrarySuffix);

                var abstractionProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.AbstractionLibrarySuffix);

                var logicProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.LogicLibrarySuffix);

                var rpcProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.GrpcServiceLibrarySuffix);

                await VisualStudioActions.UpdateGrpcServiceImplementationAsync(result,appModelProject,rpcModelProject,abstractionProject,rpcProject,logicProject,DeliveryInfo.AbstractionSuffix);

                await abstractionProject.RegisterTransientServicesAsync();

            }
            catch (CodeFactoryException automationError)
            {
                MessageBox.Show(automationError.Message, "Update Service Definition Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);  
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion
    }
}
