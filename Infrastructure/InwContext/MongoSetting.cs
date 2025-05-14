using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InwContext
{
    public class MongoSetting
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;

        public MongoSetting()
        {
            ConnectionString = "mongodb://localhost:27017";
            DatabaseName = "INWProject";
        }
    }
}
