using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Days
    {
        public Days()
        {
            RegularAvailabilities = new HashSet<RegularAvailabilities>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<RegularAvailabilities> RegularAvailabilities { get; set; }
    }
}
