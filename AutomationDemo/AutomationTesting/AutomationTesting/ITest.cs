using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTesting
{
    public interface ITest
    {
        string FirstName { get; set; }

        Task<string> GetNameAsync(Guid id, string lastName, string firstName = null);
    }
}
