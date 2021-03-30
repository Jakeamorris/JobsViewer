using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsViewer.Services
{
    class ErrorDetailsMessage
    {
        public string ErrorName { get; set; }
        public SqlException SqlException { get; set; }
    }
}
