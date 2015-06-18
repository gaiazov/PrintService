using Nancy.Hosting.Self;
using System;
using System.Configuration;

namespace PrinterService
{
    public class PrinterService
    {
        NancyHost _nancyHost;

        public void Start()
        {
            Nancy.StaticConfiguration.DisableErrorTraces = false;

            _nancyHost = new NancyHost(new Uri(ConfigurationManager.AppSettings["serviceUrl"]));
            _nancyHost.Start();
        }

        public void Stop()
        {
            _nancyHost.Stop();
        }
    }
}
