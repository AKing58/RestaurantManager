using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class TableInfo
    {
        public TableInfo()
        {
            Customer = new HashSet<Customer>();
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public int Xloc { get; set; }
        public int Yloc { get; set; }

        public virtual FurnitureType Type { get; set; }
        public virtual ICollection<Customer> Customer { get; set; }
    }
}
