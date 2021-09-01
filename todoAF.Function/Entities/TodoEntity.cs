using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoAF.Function.Entities
{
    public class TodoEntity: TableEntity
    {
        public int Idemployee { get; set; }
        public DateTime WorkingHour { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
