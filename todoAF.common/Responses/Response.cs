using System;

namespace WorkTimeEmp.common.Responses
{
    public class Response
    {
        public int Idemployee { get; set; }
        public DateTime WorkingHour { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }

    }
}
