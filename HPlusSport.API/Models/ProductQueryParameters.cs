using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPlusSport.API.Models
{
    public class ProductQueryParameters : QueryParameters
    {
        [Precision(18, 2)]
        public decimal? MinPrice { get; set; }
        [Precision(18, 2)]
        public decimal? MaxPrice { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
