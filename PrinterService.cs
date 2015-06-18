using Nancy.Hosting.Self;
using System;

namespace PrinterService
{
    public class PrinterService
    {
        NancyHost _nancyHost;

        public void Start()
        {
            _nancyHost = new NancyHost(new Uri("http://localhost:1234"));
            _nancyHost.Start();
        }

        public void Stop()
        {
            _nancyHost.Stop();
        }
    }
}
