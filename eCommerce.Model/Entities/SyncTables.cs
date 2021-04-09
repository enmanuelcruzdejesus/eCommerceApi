using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Model.Entities
{
    public class SyncTables
    {
        public int UserId { get; set; }
        public string TableName { get; set; }

        public DateTime LastUpdateSync { get; set; }

    }
}
