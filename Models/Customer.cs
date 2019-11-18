using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Customer
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int OrderId { get; set; }

        public virtual Orders Order { get; set; }
        public virtual TableInfo Table { get; set; }
    }
}
