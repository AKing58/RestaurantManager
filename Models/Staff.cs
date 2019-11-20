using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Staff
    {
        public Staff()
        {
            CurrentAvailabilities = new HashSet<CurrentAvailabilities>();
        }

        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int TitleId { get; set; }
        public decimal? Rate { get; set; }
        public string Phone { get; set; }

        public virtual Title Title { get; set; }
        public virtual ICollection<CurrentAvailabilities> CurrentAvailabilities { get; set; }
    }
}
