
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    [Table("ProductCategories")]
    public class ProductCategory
    {
      
        [Key]
        public int id { get; set; }
        public int categoryRef { get; set; }
        public string descrip { get; set; }
        public string slug { get; set; }
    }
}
