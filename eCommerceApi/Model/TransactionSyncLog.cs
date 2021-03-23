using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    public class TransactionSyncLog
    {
        [PrimaryKey]
        public int Id { get; set; }
      
        public int UserId { get; set; }
        public string TableName { get; set; }
        public string Operation { get; set; }
        public int TransId { get; set; }

        public bool IsSynchronized { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
