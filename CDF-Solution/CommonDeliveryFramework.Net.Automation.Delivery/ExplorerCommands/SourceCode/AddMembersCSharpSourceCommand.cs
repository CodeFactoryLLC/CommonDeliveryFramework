﻿using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Delivery.ExplorerCommands.SourceCode
{
 /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMembersCSharpSourceCommand : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Members";
        private static readonly string commandDescription = "Adds missing members from an interface implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMembersCSharpSourceCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                //Determines if there are missing members in the source document
                var missingMembers = result.SourceMissingInterfaceMembers();

                isEnabled = missingMembers.Any();

                if (isEnabled)
                {
                    var projectData = await result.GetHostingProjectAsync();
                    var hasLogging = await projectData.HasReferenceLibraryAsync(AspNetCoreConstants.MicrosoftLoggerLibraryName);
                    isEnabled = hasLogging;
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
                //Get the missing members and the target class or classes they are to be loaded into.
                var missingMembers = result.SourceMissingInterfaceMembers();

                //Validate there are still missing members to process.
                if(!missingMembers.Any()) return;

                //If the logging abstraction library is loaded then enable logging for members.
                var enableLogging = true;

                //Bounds checking will be supported for this command
                bool boundsChecking = true;

                //Async keyword will be used for this command
                bool supportAsync = true;

                CsSource currentSource = result.SourceCode;

                
                //Process each class missing members 
                foreach (var missingMember in missingMembers)
                {
                    //Get the container model that has missing members.
                    var container = missingMember.Key;

                    //Confirming the container is a class if not continue
                    if (container.ContainerType != CsContainerType.Class) continue;

                    var targetClass = container as CsClass;

                    
                    var returnedSource = await currentSource.AddMembersToClassAsync(targetClass, missingMember.Value, boundsChecking,
                        enableLogging, supportAsync);

                    if(returnedSource == null) return;

                    currentSource = returnedSource;
                }



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
