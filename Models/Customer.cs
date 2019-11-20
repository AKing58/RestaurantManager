using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Orders>();
        }

        public int Id { get; set; }
        public int TableId { get; set; }

        public virtual TableInfo Table { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
