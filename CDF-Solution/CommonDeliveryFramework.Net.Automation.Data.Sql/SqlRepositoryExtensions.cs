using System.Linq;
using CodeFactory.DotNet.CSharp;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Data.Sql
{
    /// <summary>
    /// Extensions and support logic class for SQL repositories
    /// </summary>
    public static class SqlRepositoryExtensions
    {
        /// <summary>
        /// Creates a Sql repository and added the CRUD operations to the repository.
        /// </summary>
        /// <param name="source">The visual studio actions.</param>
        /// <param name="sourceModel">The target sql model to implement.</param>
        /// <param name="dataContractProject">Project that contains the data contracts used by the repository.</param>
        /// <param name="dataSqlProject">The sql repository project that the repository will be created in.</param>
        /// <param name="appModelProject">The project were application models will be hosted.</param>
        /// <param name="sqlModelProject">The project that hosts the sql models.</param>
        /// <param name="repositorySuffix">The suffix that will be appended to the end of the name of the repository being created.</param>
        /// <returns>The implemented repository.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or repository definition already exists.</exception>
        public static async Task<CsClass> CreateRepositoryAsync(this IVsActions source, CsClass sourceModel,
            VsProject dataContractProject, VsProject dataSqlProject, VsProject appModelProject,
            VsProject sqlModelProject, string repositorySuffix)
        {
            // bounds checking
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot access code factories functionality to visual studio cannot create a repository");
            if (sourceModel == null)
                throw new CodeFactoryException("No sql data model was provided cannot create a repository");

            if (dataContractProject == null)
                throw new CodeFactoryException("Cannot access the data access project cannot create a repository");

            if (dataSqlProject == null)
                throw new CodeFactoryException("Cannot access the sql repository project cannot create a repository");

            if (appModelProject == null)
                throw new CodeFactoryException(
                    "Cannot access the application model project cannot create a repository");

            if (string.IsNullOrEmpty(repositorySuffix))
                throw new CodeFactoryException("No repository suffix was provided, cannot create a repository.");


            if (await dataSqlProject.FindCSharpSourceByClassNameAsync($"{sourceModel.Name}{repositorySuffix}") != null)
                throw new CodeFactoryException("The sql repository already exists.");
            if (await dataContractProject.FindCSharpSourceByInterfaceNameAsync(
                    $"I{sourceModel.Name}{repositorySuffix}") !=
                null) throw new CodeFactoryException("The sql repository contract already exists.");


            var sqlModel = sourceModel;

            var appModel = await appModelProject.FindCSharpSourceByClassNameAsync(sourceModel.Name);

            if (appModel == null)
            {
                sqlModel = await source.RefreshSqlModelAsync(sourceModel, sqlModelProject,appModelProject);

                if (sqlModel == null)
                    throw new CodeFactoryException("Could not refresh the sql model cannot create a repository");

                appModel = await appModelProject.FindCSharpSourceByClassNameAsync(sourceModel.Name);

                if (appModel == null)
                    throw new CodeFactoryException("Could not load the application model, cannot create a repository");
            }

            await sourceModel.CreateRepositoryContractAsync(dataContractProject, appModelProject, repositorySuffix);

            return await sourceModel.BuildRepositoryAsync(dataSqlProject, appModelProject, dataContractProject,sqlModelProject,repositorySuffix);
        }

        /// <summary>
        /// Creates a new interface contract definition for a repository. 
        /// </summary>
        /// <param name="sourceModel">Database entity to create a contract for.</param>
        /// <param name="contractProject">The project that stores contract definitions.</param>
        /// <param name="appModelProject">The project were application models will be hosted.</param>
        /// <param name="repositorySuffix">The suffix that will be appended to the end of the name of the repository being created.</param>
        /// <returns>Implemented interface</returns>
        private static async Task CreateRepositoryContractAsync(this CsClass sourceModel, VsProject contractProject,
            VsProject appModelProject, string repositorySuffix)
        {
            if (sourceModel == null)
                throw new CodeFactoryException("Source data entity was not provided cannot create repository contract");
            if (contractProject == null)
                throw new CodeFactoryException(
                    "The contracts project was not provided cannot create a repository contract");


            var parameterName = sourceModel.Name.FormatCamelCase();

            SourceFormatter contractFormatter = new SourceFormatter();

            contractFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            contractFormatter.AppendCodeLine(0, "using System;");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            contractFormatter.AppendCodeLine(0, "using System.Linq;");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            contractFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            contractFormatter.AppendCodeLine(0);
            contractFormatter.AppendCodeLine(0, $"namespace {contractProject.DefaultNamespace}");
            contractFormatter.AppendCodeLine(0, "{");
            contractFormatter.AppendCodeLine(1, "/// <summary>");
            contractFormatter.AppendCodeLine(1,
                $"/// Data access  contract definition that supports the app model <seealso cref=\"{sourceModel.Name}\"/>");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1, $"public interface I{sourceModel.Name}{repositorySuffix}");
            contractFormatter.AppendCodeLine(1, "{");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Inserts the {sourceModel.Name} into storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to insert.</param>");
            contractFormatter.AppendCodeLine(2, $"/// <returns>Update {sourceModel.Name} once inserted.</returns>");
            contractFormatter.AppendCodeLine(2,
                $"Task<{sourceModel.Name}> Insert{sourceModel.Name}Async({sourceModel.Name} {parameterName} );");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Updates an existing {sourceModel.Name} into storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to update.</param>");
            contractFormatter.AppendCodeLine(2, $"/// <returns>Updated {sourceModel.Name} once updated.</returns>");
            contractFormatter.AppendCodeLine(2,
                $"Task<{sourceModel.Name}> Update{sourceModel.Name}Async({sourceModel.Name} {parameterName} );");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Deletes an existing {sourceModel.Name} from storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to delete.</param>");
            contractFormatter.AppendCodeLine(2,
                $"Task Delete{sourceModel.Name}Async({sourceModel.Name} {parameterName} );");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            await contractProject.AddDocumentAsync($"I{sourceModel.Name}{repositorySuffix}.cs",
                contractFormatter.ReturnSource());

        }

        /// <summary>
        /// Builds a sql repository implementation and saves it to the target project.
        /// </summary>
        /// <param name="sourceModel">The source entity to generate the repository from.</param>
        /// <param name="repositoryProject">The project that will host the repository.</param>
        /// <param name="appModelProject">The project were application models will be hosted.</param>
        /// <param name="dataContractProject">Project that contains the data contracts used by the repository.</param>
        /// <param name="sqlModelProject">Project that holds the sql data models</param>
        /// <param name="repositorySuffix">The suffix that will be appended to the end of the name of the repository being created.</param>
        /// <returns>Implemented repository.</returns>
        /// <exception cref="CodeFactoryException">If the source or project are missing.</exception>
        private static async Task<CsClass> BuildRepositoryAsync(this CsClass sourceModel, VsProject repositoryProject,
            VsProject appModelProject, VsProject dataContractProject, VsProject sqlModelProject, string repositorySuffix)
        {
            if (sourceModel == null)
                throw new CodeFactoryException("Source data entity was not provided cannot create repository contract");
            if (repositoryProject == null)
                throw new CodeFactoryException(
                    "The repository project was not provided cannot create a repository contract");

            var dbContext = await sourceModel.GetEfContextClassAsync(sqlModelProject);

            if (dbContext == null)
                throw new CodeFactoryException("Could not locate the entity framework context that supports this data model. Cannot create the sql repository.");

            string dbContextName = $"{dbContext.Namespace}.{dbContext.Name}";

            var parameterName = sourceModel.Name.FormatCamelCase();

            SourceFormatter contractFormatter = new SourceFormatter();

            contractFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            contractFormatter.AppendCodeLine(0, "using System;");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            contractFormatter.AppendCodeLine(0, "using System.Data;");
            contractFormatter.AppendCodeLine(0, "using System.Linq;");
            contractFormatter.AppendCodeLine(0, "using System.Runtime.InteropServices;");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            contractFormatter.AppendCodeLine(0, "using Dapper;");
            contractFormatter.AppendCodeLine(0, "using Microsoft.Data.SqlClient;");
            contractFormatter.AppendCodeLine(0, "using Microsoft.EntityFrameworkCore;");
            contractFormatter.AppendCodeLine(0, "using Microsoft.EntityFrameworkCore.Design;");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Database;");
            contractFormatter.AppendCodeLine(0, $"using {dataContractProject.DefaultNamespace};");
            contractFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            contractFormatter.AppendCodeLine(0);
            contractFormatter.AppendCodeLine(0, $"namespace {repositoryProject.DefaultNamespace}");
            contractFormatter.AppendCodeLine(0, "{");
            contractFormatter.AppendCodeLine(1, "/// <summary>");
            contractFormatter.AppendCodeLine(1,
                $"/// Data access repository implementation for contract <seealso cref=\"I{sourceModel.Name}{repositorySuffix}\"/>");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1,
                $"public class {sourceModel.Name}{repositorySuffix}:I{sourceModel.Name}{repositorySuffix}");
            contractFormatter.AppendCodeLine(1, "{");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Logger used for the class. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Database connection information. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"private readonly DbConnectionString<{dbContextName}> _dbConnectionString;");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, "/// Creates a instance of the repository.");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                "/// <param name=\"logger\">Logger that supports this abstraction.</param>");
            contractFormatter.AppendCodeLine(2, "/// <param name=\"dbConnectionString\">database data source.</param>");
            contractFormatter.AppendCodeLine(2,
                $"public {sourceModel.Name}{repositorySuffix}(ILogger<{sourceModel.Name}{repositorySuffix}> logger, DbConnectionString<{dbContextName}> dbConnectionString)");
            contractFormatter.AppendCodeLine(2, "{");
            contractFormatter.AppendCodeLine(3, "_logger = logger;");
            contractFormatter.AppendCodeLine(3, "_dbConnectionString = dbConnectionString;");
            contractFormatter.AppendCodeLine(2, "}");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Inserts the {sourceModel.Name} into storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to insert.</param>");
            contractFormatter.AppendCodeLine(2, $"/// <returns>Update {sourceModel.Name} once inserted.</returns>");
            contractFormatter.AppendCodeLine(2,
                $"public async Task<{sourceModel.Name}> Insert{sourceModel.Name}Async({sourceModel.Name} {parameterName} )");
            contractFormatter.AppendCodeLine(2, "{");
            contractFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, $"if({parameterName} == null)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"_logger.ErrorLog($\"The parameter {{ nameof({parameterName})}} was not provided.Will raise an argument exception\");");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, $"throw new CommonDeliveryFramework.ValidationException(nameof({parameterName}));");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, $"{sourceModel.Name} result = null;");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "try");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"using (var context = new {dbContextName}(_dbConnectionString.ConnectionString))");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                $"var insertData = {sourceModel.Namespace}.{sourceModel.Name}.CreateSqlModel({parameterName});");
            contractFormatter.AppendCodeLine(5);
            contractFormatter.AppendCodeLine(5,
                $"var data = await context.{sourceModel.Name.EfDataSetName()}.AddAsync(insertData);");
            contractFormatter.AppendCodeLine(5, "var resultCode = await context.SaveChangesAsync();");
            contractFormatter.AppendCodeLine(5, "result = data.Entity.CreateAppModel();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (ManagedException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4,
                "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
            contractFormatter.AppendCodeLine(4, "throw;");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (DbUpdateException updateException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "var sqlError = updateException.InnerException as SqlException;");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4, "if (sqlError == null)");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                "_logger.ErrorLog(\"The following database error occurred.\", updateException);");
            contractFormatter.AppendCodeLine(5, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(5, "throw new CommonDeliveryFramework.DataException();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4,
                "_logger.ErrorLog(\"The following SQL exception occurred.\", sqlError);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "sqlError.GenerateManagedException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                "_logger.CriticalLog( \"An unhandled exception occurred, see the exception for details.Will throw a UnhandledException\", unhandledException);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "throw new CommonDeliveryFramework.UnhandledException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(3, "return result;");
            contractFormatter.AppendCodeLine(2, "}");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Updates an existing {sourceModel.Name} into storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to update.</param>");
            contractFormatter.AppendCodeLine(2, $"/// <returns>Updated {sourceModel.Name} once updated.</returns>");
            contractFormatter.AppendCodeLine(2,
                $"public async Task<{sourceModel.Name}> Update{sourceModel.Name}Async({sourceModel.Name} {parameterName} )");
            contractFormatter.AppendCodeLine(2, "{");
            contractFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, $"if({parameterName} == null)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"_logger.ErrorLog($\"The parameter {{ nameof({parameterName})}} was not provided.Will raise an argument exception\");");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, $"throw new CommonDeliveryFramework.ValidationException(nameof({parameterName}));");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, $"{sourceModel.Name} result = null;");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "try");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"using (var context = new {dbContextName}(_dbConnectionString.ConnectionString))");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                $"var updateData = {sourceModel.Namespace}.{sourceModel.Name}.CreateSqlModel({parameterName});");
            contractFormatter.AppendCodeLine(5);
            contractFormatter.AppendCodeLine(5,
                $"var data = context.{sourceModel.Name.EfDataSetName()}.Update(updateData);");
            contractFormatter.AppendCodeLine(5, "var resultCode = await context.SaveChangesAsync();");
            contractFormatter.AppendCodeLine(5, "result = data.Entity.CreateAppModel();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (ManagedException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4,
                "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
            contractFormatter.AppendCodeLine(4, "throw;");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (DbUpdateException updateException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "var sqlError = updateException.InnerException as SqlException;");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4, "if (sqlError == null)");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                "_logger.ErrorLog(\"The following database error occurred.\", updateException);");
            contractFormatter.AppendCodeLine(5, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(5, "throw new CommonDeliveryFramework.DataException();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4,
                "_logger.ErrorLog(\"The following SQL exception occurred.\", sqlError);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "sqlError.GenerateManagedException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                "_logger.CriticalLog( \"An unhandled exception occurred, see the exception for details.Will throw a UnhandledException\", unhandledException);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "throw new CommonDeliveryFramework.UnhandledException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(3, "return result;");
            contractFormatter.AppendCodeLine(2, "}");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Deletes an existing {sourceModel.Name} from storage. />");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2,
                $"/// <param name=\"{parameterName}\">{sourceModel.Name} to delete.</param>");
            contractFormatter.AppendCodeLine(2,
                $"public async Task Delete{sourceModel.Name}Async({sourceModel.Name} {parameterName} )");
            contractFormatter.AppendCodeLine(2, "{");
            contractFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, $"if({parameterName} == null)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"_logger.ErrorLog($\"The parameter {{ nameof({parameterName})}} was not provided.Will raise an argument exception\");");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, $"throw new CommonDeliveryFramework.ValidationException(nameof({parameterName}));");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "try");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                $"using (var context = new {dbContextName}(_dbConnectionString.ConnectionString))");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                $"var deleteData = {sourceModel.Namespace}.{sourceModel.Name}.CreateSqlModel({parameterName});");
            contractFormatter.AppendCodeLine(5);
            contractFormatter.AppendCodeLine(5,
                $"var data = context.{sourceModel.Name.EfDataSetName()}.Remove(deleteData);");
            contractFormatter.AppendCodeLine(5, "var resultCode = await context.SaveChangesAsync();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (ManagedException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4,
                "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
            contractFormatter.AppendCodeLine(4, "throw;");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (DbUpdateException updateException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4, "var sqlError = updateException.InnerException as SqlException;");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4, "if (sqlError == null)");
            contractFormatter.AppendCodeLine(4, "{");
            contractFormatter.AppendCodeLine(5,
                "_logger.ErrorLog(\"The following database error occurred.\", updateException);");
            contractFormatter.AppendCodeLine(5, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(5, "throw new CommonDeliveryFramework.DataException();");
            contractFormatter.AppendCodeLine(4, "}");
            contractFormatter.AppendCodeLine(4);
            contractFormatter.AppendCodeLine(4,
                "_logger.ErrorLog(\"The following SQL exception occurred.\", sqlError);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "sqlError.GenerateManagedException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
            contractFormatter.AppendCodeLine(3, "{");
            contractFormatter.AppendCodeLine(4,
                "_logger.CriticalLog( \"An unhandled exception occurred, see the exception for details.Will throw a UnhandledException\", unhandledException);");
            contractFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(4, "throw new CommonDeliveryFramework.UnhandledException();");
            contractFormatter.AppendCodeLine(3, "}");
            contractFormatter.AppendCodeLine(3);
            contractFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
            contractFormatter.AppendCodeLine(2, "}");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var doc = await repositoryProject.AddDocumentAsync($"{sourceModel.Name}{repositorySuffix}.cs",
                contractFormatter.ReturnSource());

            var sourceDocument = await doc.GetCSharpSourceModelAsync();

            return sourceDocument.Classes.FirstOrDefault();
        }

        public static string FormatRepositoryMethod(this CsMethod memberData, NamespaceManager manager,
            bool includeLogging,
            bool includeBoundsCheck, bool supportAsyncKeyword)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a method signature.
            string methodSyntax = supportAsyncKeyword
                ? memberData.CSharpFormatStandardMethodSignatureWithAsync(manager)
                : memberData.CSharpFormatStandardMethodSignature(manager);

            //If the method syntax was not created return.
            if (string.IsNullOrEmpty(methodSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(
                             manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the method declaration
            formatter.AppendCodeLine(0, methodSyntax);
            formatter.AppendCodeLine(0, "{");

            //Adding enter logging if logging is enabled.
            if (includeLogging)
            {
                formatter.AppendCodeLine(1, "_logger.InformationEnterLog();");
                formatter.AppendCodeLine(0);
            }

            //Processing each parameter for bounds checking if bounds checking is enabled.
            if (includeBoundsCheck & memberData.HasParameters)
            {

                foreach (ICsParameter paramData in memberData.Parameters)
                {
                    //If the parameter has a default value then continue will not bounds check parameters with a default value.
                    if (paramData.HasDefaultValue) continue;

                    //If the parameter is a string type add the following bounds check
                    if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if(string.IsNullOrEmpty({paramData.Name}))");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new CommonDeliveryFramework.ValidationException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }

                    // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                    if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if({paramData.Name} == null)");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ValidationException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }
                }
            }

            formatter.AppendCodeLine(1, "try");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "// Add logic here.");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(1, "catch (ManagedException)");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
            formatter.AppendCodeLine(2,
                "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
            formatter.AppendCodeLine(2, "throw;");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(1, "catch (SqlException sqlDataException)");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2,
                "_logger.ErrorLog(\"The following SQL exception occurred.\", sqlDataException);");
            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
            formatter.AppendCodeLine(2, "sqlDataException.GenerateManagedException();");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(1, "catch (DbUpdateException updateException)");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "var sqlError = updateException.InnerException as SqlException;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2, "if (sqlError == null)");
            formatter.AppendCodeLine(2, "{");
            formatter.AppendCodeLine(3,
                "_logger.ErrorLog(\"The following database error occurred.\", updateException);");
            formatter.AppendCodeLine(3, "_logger.InformationExitLog();");
            formatter.AppendCodeLine(3, "throw new CommonDeliveryFramework.CommonDeliveryFramework.DataException();");
            formatter.AppendCodeLine(2, "}");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2, "_logger.ErrorLog(\"The following SQL exception occurred.\", sqlError);");
            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
            formatter.AppendCodeLine(2, "sqlError.GenerateManagedException();");
            formatter.AppendCodeLine(2, "}");
            formatter.AppendCodeLine(1, "catch (Exception unhandledException)");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2,
                "_logger.CriticalLog( \"An unhandled exception occurred, see the exception for details.Will throw a UnhandledException\", unhandledException);");
            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
            formatter.AppendCodeLine(2, "throw new CommonDeliveryFramework.UnhandledException();");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(1);

            //If logging add a logging exit statement.
            formatter.AppendCodeLine(1, "_logger.InformationExitLog();");

            //Add an exception for not implemented until the developer updates the logic.
            formatter.AppendCodeLine(1, "throw new CommonDeliveryFramework.NotImplementedException();");

            //if the return type is not void then add a to do message for the developer to add return logic.
            if (!memberData.IsVoid)
            {
                formatter.AppendCodeLine(0);
                formatter.AppendCodeLine(1, "//TODO: add return logic here");
            }

            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            //Returning the fully formatted method.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Gets the hosting entity framework context that hosts this sql data model class.
        /// </summary>
        /// <param name="source">Sql data model that is being hosted in the context.</param>
        /// <param name="sqlModelProject">The model project where the sql model and context are hosted in.</param>
        /// <returns>The context class or null if it could not be found.</returns>
        public static async Task<CsClass> GetEfContextClassAsync(this CsClass source, VsProject sqlModelProject)
        {
            if (source == null) return null;

            if (sqlModelProject == null) return null;

            var children = await sqlModelProject.GetChildrenAsync(true, true);

            var classes = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>()
                .Where(s => s.SourceCode.Classes.Any()).Select(x => x);

            CsClass result = null;

            foreach (var classSource in classes)
            {
                var dbContext = classSource.SourceCode.Classes.FirstOrDefault(c => c.BaseClass.Namespace == "Microsoft.EntityFrameworkCore" & c.BaseClass.Name == "DbContext" & c.Properties.Any());

                if(dbContext == null) continue;

                bool hasDataClass = dbContext.Properties
                    .Where(p => p.PropertyType.Namespace == "Microsoft.EntityFrameworkCore" &
                                p.PropertyType.Name == "DbSet" & p.PropertyType.IsGeneric).Any(p =>
                        p.PropertyType.GenericTypes[0]?.Namespace == source.Namespace &
                        p.PropertyType.GenericTypes[0]?.Name == source.Name);

                if (hasDataClass) result = dbContext;
            }

            return result;
        }

        /// <summary>
        /// Helper method that puts the correct plural form on the data set name.
        /// </summary>
        /// <param name="className">Data set name to evaluate.</param>
        /// <returns>Formatted data set name.</returns>
        private static string EfDataSetName(this string className)
        {
            if (className == null) return null;

            if (className.EndsWith("y"))
            {

                if (className.EndsWith("ay")) return $"{className}s";
                if (className.EndsWith("ey")) return $"{className}s";
                return $"{className.Substring(0, className.Length - 1)}ies";
            }

            if (className.EndsWith("s")) return $"{className}es";
            return $"{className}s";
        }
    }
}