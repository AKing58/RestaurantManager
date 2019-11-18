using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Orders
    {
        public Orders()
        {
            Customer = new HashSet<Customer>();
        }

        public int Id { get; set; }
        public int ItemId { get; set; }

        public virtual Item Item { get; set; }
        public virtual ICollection<Customer> Customer { get; set; }
    }
}
