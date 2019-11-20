using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Item
    {
        public Item()
        {
            Orders = new HashSet<Orders>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }

        public virtual ICollection<Orders> Orders { get; set; }
    }
}
