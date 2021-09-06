using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WorkTimeEmp.Function.Entities
{
    public class WorkTimeEmpEntity : TableEntity
    {
        public int Idemployee { get; set; }
        public DateTime WorkingHour { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
