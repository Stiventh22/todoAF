using System;

namespace todoAF.common.Models
{
    public class Todo
    {
        public int Idemployee { get; set; }
        public DateTime WorkingHour { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
