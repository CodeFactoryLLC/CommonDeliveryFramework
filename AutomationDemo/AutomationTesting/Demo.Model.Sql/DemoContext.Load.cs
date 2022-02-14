using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Model.Sql
{
    public partial class DemoContext
    {

        private readonly string _sqlConnectionString;

        public DemoContext(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }

    }
}
