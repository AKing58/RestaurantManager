using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class CurrentSchedule
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? BlockStartTime { get; set; }
        public TimeSpan? BlockEndTime { get; set; }

        public virtual Staff Staff { get; set; }
    }
}
