using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    public class SyncTables
    {
        public int UserId { get; set; }
        public string TableName { get; set; }

        public DateTime LastUpdateSync { get; set; }

    }
}
