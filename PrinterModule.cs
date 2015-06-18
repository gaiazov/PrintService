using Nancy;
using Nancy.ModelBinding;
using PrinterService.Model;
using System.Collections.Generic;

namespace PrinterService
{
    public class PrinterModule : NancyModule
    {
        public PrinterModule()
        {
            Post["/print"] = parameters =>
            {
                var request = this.Bind<PrintRequest>();

                Printer printer = new Printer("Hengstler Extendo X56");

                foreach (var cookie in request.Cookies)
                {
                    printer.SetCookie(cookie.Key, cookie.Value);
                }

                printer.PrintPdf(request.Url);

                return Response.AsJson<PrintOutcome>(new PrintOutcome()
                {
                    Printed = true,
                    Message = "Printed Successfully"
                }, HttpStatusCode.OK);
            };
        }
    }
}
