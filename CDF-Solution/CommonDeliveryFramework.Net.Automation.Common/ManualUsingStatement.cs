using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.SourceCode;

namespace CommonDeliveryFramework.Net.Automation.Common
{
    /// <summary>
    /// For use when building manual namespaces
    /// </summary>
    public class ManualUsingStatement : CsUsingStatement
    {
        /// <summary>
        /// Creates a new instance of a manual namespace. for use with namespace manager only.
        /// </summary>
        /// <param name="referenceNamespace">The target namespace to use with formatting.</param>
        /// <param name="hasAlias">If the namespace is set with an alias</param>
        /// <param name="alias">The alias for the namespace.</param>
        public ManualUsingStatement(string referenceNamespace, bool hasAlias = false, string alias = null) : base(true,false,true, SourceCodeType.CSharp, "manual", referenceNamespace, hasAlias, alias,"manualParent" ,"", null,null)
        {
            //Intentionally blank
        }

        public override Task<CsSource> AddAfterAsync(string sourceDocument, string sourceCode)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> AddAfterAsync(string sourceCode)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> AddBeforeAsync(string sourceDocument, string sourceCode)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> AddBeforeAsync(string sourceCode)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> DeleteAsync(string sourceDocument)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<ModelLoadException> GetErrors()
        {
            throw new NotImplementedException();
        }

        public override Task<ISourceLocation> GetSourceLocationAsync(string sourceDocument)
        {
            throw new NotImplementedException();
        }

        public override Task<ISourceLocation> GetSourceLocationAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> ReplaceAsync(string sourceDocument, string sourceCode)
        {
            throw new NotImplementedException();
        }

        public override Task<CsSource> ReplaceAsync(string sourceCode)
        {
            throw new NotImplementedException();
        }
    }
}