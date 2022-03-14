using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;

namespace CommonDeliveryFramework.Net.Automation.Common
{
    /// <summary>
    /// Extension methods that support actions related to projects or from project models.
    /// </summary>
    public static class VsProjectExtensions
    {
        /// <summary>
        /// Locates a target model that implements the transformation for a source model.
        /// </summary>
        /// <param name="source">The project to search.</param>
        /// <param name="className">The name of the class that is managed in the source control file.</param>
        /// <returns>The source code file the target class was found in.</returns>
        public static async Task<VsCSharpSource> FindCSharpSourceByClassNameAsync(this VsProject source, string className)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(className)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => s.SourceCode.Classes.Any(c => c.Name == className));
            
            return result;
        }

        /// <summary>
        /// Locates a target model that implements the transformation for a source model.
        /// </summary>
        /// <param name="source">The project folder to search.</param>
        /// <param name="className">The name of the class that is managed in the source control file.</param>
        /// <returns>The source code file the target class was found in.</returns>
        public static async Task<VsCSharpSource> FindCSharpSourceByClassNameAsync(this VsProjectFolder source, string className)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(className)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => s.SourceCode.Classes.Any(c => c.Name == className));

            return result;
        }

        /// <summary>
        /// Locates a target model that implements the transformation for a source model.
        /// </summary>
        /// <param name="source">The project to search.</param>
        /// <param name="name">The name of the interface that is managed in the source control file.</param>
        /// <returns>The source code file the target interface was found in.</returns>
        public static async Task<VsCSharpSource> FindCSharpSourceByInterfaceNameAsync(this VsProject source, string name)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(name)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => s.SourceCode.Interfaces.Any(c => c.Name == name));

            return result;
        }

        /// <summary>
        /// Locates a target model that implements the transformation for a source model.
        /// </summary>
        /// <param name="source">The project folder to search.</param>
        /// <param name="name">The name of the interface that is managed in the source control file.</param>
        /// <returns>The source code file the target interface was found in.</returns>
        public static async Task<VsCSharpSource> FindCSharpSourceByInterfaceNameAsync(this VsProjectFolder source, string name)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(name)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => s.SourceCode.Interfaces.Any(c => c.Name == name));

            return result;
        }

        /// <summary>
        /// Locates a target source by the filename of the source code file.
        /// </summary>
        /// <param name="source">The project to search.</param>
        /// <param name="fileName">The name of the source code file.</param>
        /// <returns>The source code file the target class was found in.</returns>
        public static async Task<VsCSharpSource> FindCSharpSourceByFileNameAsync(this VsProject source, string fileName)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(fileName)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => Path.GetFileName(s.SourceCode.SourceDocument) == fileName);

            return result;
        }

        /// <summary>
        /// Gets the target project from the solution by name of the project.
        /// </summary>
        /// <param name="source">Visual studio actions to get the project from.</param>
        /// <param name="projectName">The name of the project to load from.</param>
        /// <returns>The target project or null if the project cannot be found. </returns>
        public static async Task<VsProject> GetTargetProjectAsync(this IVsActions source, string projectName)
        {
            if (source == null) return null;
            if (string.IsNullOrEmpty(projectName)) return null;

            var solution = await source.GetSolutionAsync();

            var projects = await solution.GetProjectsAsync(true);

            return projects.FirstOrDefault(p => p.Name == projectName);
        }

        /// <summary>
        /// Gets the first target project from the solution by suffix name of the project.
        /// </summary>
        /// <param name="source">Visual studio actions to get the project from.</param>
        /// <param name="projectNameSuffix">The suffix of the project name to load.</param>
        /// <returns>The target project or null if the project cannot be found. </returns>
        public static async Task<VsProject> GetTargetProjectBySuffixAsync(this IVsActions source, string projectNameSuffix)
        {
            if (source == null) return null;
            if (string.IsNullOrEmpty(projectNameSuffix)) return null;

            var solution = await source.GetSolutionAsync();

            var projects = await solution.GetProjectsAsync(true);

            return projects.FirstOrDefault(p => p.Name.EndsWith(projectNameSuffix));
        }

        /// <summary>
        /// Get the full project folder that exists under the project the source file is hosted in. 
        /// </summary>
        /// <param name="source">The source C# source code to get the directory structure for.</param>
        /// <returns>Read only list in folder order hosted under the target project.</returns>
        public static async Task<IReadOnlyList<string>> GetProjectFolderStructureAsync(this VsCSharpSource source)
        {
            var documentData = await source.LoadDocumentModelAsync();

            var resultData = new Stack<string>();

            if (!documentData.HasParent) return ImmutableList<string>.Empty;

            bool hasProcessedAllParentElements = false;

            var parentModel = await documentData.GetParentAsync();

            while (!hasProcessedAllParentElements)
            {
                if (parentModel.ModelType == VisualStudioModelType.ProjectFolder)
                {
                    if (parentModel is VsProjectFolder folderData)
                    {
                        resultData.Push(folderData.Name);

                        if (!folderData.HasParent)
                        {
                            hasProcessedAllParentElements = true;
                            continue;
                        }

                        parentModel = await folderData.GetParentAsync();
                    }
                }

                hasProcessedAllParentElements = true;
            }

            return resultData.Any() ? resultData.ToImmutableList() : ImmutableList<string>.Empty;
        }

        /// <summary>
        /// Finds the source code for a target class in a project.
        /// </summary>
        /// <param name="source">The project to search.</param>
        /// <param name="sourceClass">The class model to find the source file for.</param>
        /// <returns>The source code file the target model was found in.</returns>
        public static async Task<VsCSharpSource> FindSource(this VsProject source, CsClass sourceClass)
        {

            //Bounds checking
            if (source == null) return null;
            if (sourceClass == null) return null;

            string sourcePath = sourceClass.SourceDocument;

            if (string.IsNullOrEmpty(sourcePath)) sourcePath = sourceClass.SourceFiles.FirstOrDefault();

            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new CodeFactoryException($"Could not Find the source code file for source class '{sourceClass.Namespace}.{sourceClass.Name}' operation could not complete.");

            }

            var children = await source.GetChildrenAsync(true, true);

            return children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>()
                .FirstOrDefault(d => d.SourceCode.SourceDocument == sourcePath);

        }

    }
}