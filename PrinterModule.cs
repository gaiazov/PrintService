using Nancy;
using Nancy.ModelBinding;
using PrinterService.Model;
using System.Configuration;

namespace PrinterService
{
    public class PrinterModule : NancyModule
    {
        public PrinterModule()
        {
            Post["/print", true] = async (ctx, ct) =>
            {
                var request = this.Bind<PrintRequest>();

                var client = new Downloader();
                foreach (var cookie in request.Cookies)
                {
                    client.AddCookie(cookie.Key, cookie.Value);
                }

                byte[] pdf = await client.DownloadPDF(request.Url);


                Printer printer = new Printer(ConfigurationManager.AppSettings["servicePrinter"]);
                printer.PrintPdf(pdf);

                return Response.AsJson<PrintOutcome>(new PrintOutcome()
                {
                    Printed = true,
                    Message = "Printed Successfully"
                }, HttpStatusCode.OK);
            };
        }
    }
}
