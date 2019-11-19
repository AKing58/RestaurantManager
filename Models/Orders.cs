using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Orders
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public int ItemId { get; set; }

        public virtual Customer Cust { get; set; }
        public virtual Item Item { get; set; }
    }
}
