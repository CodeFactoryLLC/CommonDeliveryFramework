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
using CodeFactory.DotNet.CSharp;
using CommonDeliveryFramework.Net.Automation.Common;
using CommonDeliveryFramework.Net.Automation.Delivery.Logic;
using CommonDeliveryFramework.Net.Automation.Data.Sql;

namespace CommonDeliveryFramework.Net.Automation.Delivery.ExplorerCommands.SourceCode
{
/// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class CreateSqlRepositoryCsDocumentCommand : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Create Sql Repository";
        private static readonly string commandDescription = "Creates a new Sql repository and the contract for the repository";

#pragma warning disable CS1998

        /// <inheritdoc />
        public CreateSqlRepositoryCsDocumentCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                isEnabled = await result.IsSqlModelProjectAsync();

                CsClass sqlModel = null;
                if (isEnabled)
                {
                    sqlModel = result.SourceCode.Classes.FirstOrDefault();

                    isEnabled = sqlModel != null;
                }

                if (isEnabled)
                {
                    var repositoryContract = $"I{sqlModel.Name}{DeliveryInfo.RepositorySuffix}";

                    var contractProject =
                        await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.DataContractLibrarySuffix);

                    if (contractProject == null) isEnabled = false;
                    else isEnabled =  await contractProject.FindCSharpSourceByInterfaceNameAsync(repositoryContract) == null;
                }

                if (isEnabled)
                {
                    var repository = $"{sqlModel.Name}{DeliveryInfo.RepositorySuffix}";

                    var repositoryProject =
                        await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.SqlDataRepositorySuffix);
                    if (repositoryProject == null) isEnabled = false;
                    else isEnabled = await repositoryProject.FindCSharpSourceByClassNameAsync(repository) == null;
                }
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

                var dataContractProject = await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.DataContractLibrarySuffix);

                var sqlModelProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.SqlModelLibrarySuffix);

                var sqlRepositoryProject =
                    await VisualStudioActions.GetTargetProjectBySuffixAsync(DeliveryInfo.SqlDataRepositorySuffix);

                await VisualStudioActions.CreateRepositoryAsync(result.SourceCode.Classes.FirstOrDefault(),dataContractProject,
                    sqlRepositoryProject,appModelProject,sqlModelProject,DeliveryInfo.RepositorySuffix);

                await sqlRepositoryProject.RegisterTransientServicesAsync();

            }
            catch (CodeFactoryException factoryError)
            {
                MessageBox.Show(factoryError.Message, "Automation Error",MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
