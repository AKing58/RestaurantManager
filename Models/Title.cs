using System;
using System.Collections.Generic;

namespace COMP4952.Models
{
    public partial class Title
    {
        public Title()
        {
            Staff = new HashSet<Staff>();
        }

        public int Id { get; set; }
        public string Title1 { get; set; }

        public virtual ICollection<Staff> Staff { get; set; }
    }
}
