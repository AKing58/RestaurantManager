using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class CurrentAvailabilities
    {
        public CurrentAvailabilities()
        {
            CurrentSchedule = new HashSet<CurrentSchedule>();
        }

        public int Id { get; set; }
        public int StaffId { get; set; }
        public DateTime BlockStartTime { get; set; }
        public DateTime BlockEndTime { get; set; }

        public virtual Staff Staff { get; set; }
        public virtual ICollection<CurrentSchedule> CurrentSchedule { get; set; }
    }
}
