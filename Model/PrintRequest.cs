using System.Collections.Generic;

namespace PrinterService.Model
{
    public class PrintRequest
    {
        public string Url { get; set; }
        public IEnumerable<PrintCookie> Cookies { get; set; }

        public class PrintCookie
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
