using System;

namespace WorkTimeEmp.common.Models
{
    public class WorkTimeEmp
    {
        public int Idemployee { get; set; }
        public DateTime WorkingHour { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
