using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTimeEmp.Function.Entities
{
    public class ConsolEntity: TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime DateTime { get; set; }
        public double MinuteTime { get; set; }
    }
}
