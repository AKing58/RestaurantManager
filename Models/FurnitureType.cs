using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class FurnitureType
    {
        public FurnitureType()
        {
            TableInfo = new HashSet<TableInfo>();
        }

        public int Id { get; set; }
        public string Type { get; set; }

        public virtual ICollection<TableInfo> TableInfo { get; set; }
    }
}
