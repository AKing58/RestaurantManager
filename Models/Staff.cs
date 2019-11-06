using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Staff
    {
        public Staff()
        {
            CurrentSchedule = new HashSet<CurrentSchedule>();
            RegularAvailabilities = new HashSet<RegularAvailabilities>();
        }

        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int TitleId { get; set; }
        public decimal? Rate { get; set; }
        public string Phone { get; set; }

        public virtual Title Title { get; set; }
        public virtual ICollection<CurrentSchedule> CurrentSchedule { get; set; }
        public virtual ICollection<RegularAvailabilities> RegularAvailabilities { get; set; }
    }
}
