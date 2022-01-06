using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;

namespace CommonDeliveryFramework.Net.Automation.Common
{

    /// <summary>
    /// Creates a namespace manager and manually load the namespace manager before using with automation.
    /// </summary>
    public class ManualNamespaceManager
    {
        public ManualNamespaceManager()
        {
            _usingStatements = new List<CsUsingStatement>();
        }
        public ManualNamespaceManager(string defaultNamespace)
        {
            _usingStatements = new List<CsUsingStatement>();
            DefaultNamespace = defaultNamespace;
        }
        private readonly List<CsUsingStatement> _usingStatements;

        public void AddUsingStatement(string nameSpace, bool hasAlias = false, string alias = null)
        {
            if (!_usingStatements.Any(u => u.ReferenceNamespace == nameSpace & u.Alias == alias)) _usingStatements.Add(new ManualUsingStatement(nameSpace, hasAlias, alias));
        }

        public void AddExistingUsingStatements(IReadOnlyList<CsUsingStatement> usingStatements)
        {
            foreach (CsUsingStatement usingStatement in usingStatements)
            {
                if (!_usingStatements.Any(u => u.ReferenceNamespace == usingStatement.ReferenceNamespace & u.Alias == usingStatement.Alias)) _usingStatements.Add(usingStatement);
            }
        }

        public string DefaultNamespace { get; set; }

        public CodeFactory.Formatting.CSharp.NamespaceManager BuildNamespaceManager()
        {
            return new CodeFactory.Formatting.CSharp.NamespaceManager(_usingStatements.ToImmutableList(), DefaultNamespace);
        }

    }
}
