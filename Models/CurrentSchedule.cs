using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class CurrentSchedule
    {
        public int Id { get; set; }
        public int AvailabilityId { get; set; }
        public DateTime BlockStartTime { get; set; }
        public DateTime BlockEndTime { get; set; }

        public virtual CurrentAvailabilities Availability { get; set; }
    }
}
