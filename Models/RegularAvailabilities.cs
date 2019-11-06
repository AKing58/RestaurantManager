using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class RegularAvailabilities
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int DayId { get; set; }
        public TimeSpan BlockStartTime { get; set; }
        public TimeSpan BlockEndTime { get; set; }

        public virtual Days Day { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
