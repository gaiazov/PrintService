using Flurl.Http;
using Ghostscript.NET.Rasterizer;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;

namespace PrinterService
{
    public class Printer
    {
        private PrintDocument _printer;

        private List<Bitmap> _pages;
        private int _pageNum = 0;

        private Dictionary<string, string> _cookies = new Dictionary<string, string>();

        public Printer(string printerName)
        {
            _printer = new PrintDocument()
            {
                PrintController = new StandardPrintController(),
                OriginAtMargins = false,
                DefaultPageSettings = new PageSettings()
                {
                    Margins = new Margins(0, 0, 0, 0)
                },
                PrinterSettings = new PrinterSettings()
                {
                    PrinterName = printerName
                }
            };

            _printer.PrintPage += new PrintPageEventHandler(PrintPage);
        }

        public void SetCookie(string name, string value)
        {
            _cookies.Add(name, value);
        }

        public void PrintPdf(string url)
        {
            var bufferTask = new FlurlClient(url)
                .WithCookies(_cookies)
                .GetBytesAsync();

            var buffer = bufferTask.Result;

            int desired_x_dpi = 203;
            int desired_y_dpi = 203;

            _pages = new List<Bitmap>();

            using (var memoryStream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(buffer);
                using (PdfStamper stamper = new PdfStamper(reader, memoryStream))
                {
                    PdfReaderContentParser parser = new PdfReaderContentParser(reader);

                    int n = reader.NumberOfPages;
                    for (int i = 1; i <= n; i++)
                    {

                        TextMarginFinder finder = parser.ProcessContent(i, new TextMarginFinder());
                        var cropBox = reader.GetCropBox(i);

                        var margin = finder.GetLlx();

                        cropBox.Bottom = finder.GetLly() - margin;
                        reader.GetPageN(i).Put(PdfName.CROPBOX, new PdfRectangle(cropBox));
                    }
                }

                buffer = memoryStream.GetBuffer();
            }


            using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
            {
                MemoryStream ms = new MemoryStream(buffer);

                rasterizer.Open(ms);

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    var img = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);
                    _pages.Add(new Bitmap(img));
                }
            }
            
            for (int i = 0; i < _pages.Count(); i++)
            {
                PrintSinglePage(i);
            }
        }

        private void PrintSinglePage(int pageNum)
        {
            Console.WriteLine("Printing Single Page");
            _pageNum = pageNum;

            var image = _pages[pageNum];

            float heightInches = image.Height / 203f;
            float widthInches = image.Width / 203f;

            var pagePaperSize = new PaperSize("Page " + 0 + " Custom Size", (int)(widthInches * 100), (int)(heightInches * 100));
            _printer.DefaultPageSettings.PaperSize = pagePaperSize;
            _printer.Print();
            Console.WriteLine("Printing Single Page.... Done");
        }

        private void PrintImage(Bitmap image, PrintPageEventArgs args)
        {
            float aspectRatio = image.Height / (float)image.Width;

            Graphics g = args.Graphics;

            float height = g.VisibleClipBounds.Width * aspectRatio;
            float width = g.VisibleClipBounds.Width;

            g.DrawImage(image, 0.0f, 0.0f, width, height);

            // draw a line at the bottom of the image
            // so the printer scrolls to it
            // white space at the end of the printed page causes issues
            g.DrawLine(new Pen(Color.Gray), 0, height, width/10, height);
        }

        private void PrintPage(object sender, PrintPageEventArgs args)
        {
            Console.WriteLine("Page " + (_pageNum + 1));

            var bitmap = _pages[_pageNum];

            // set NEXT page size
            {
                if ((_pageNum + 1) < _pages.Count())
                {
                    var bitmapNext = _pages[_pageNum + 1];

                    float heightInches = bitmapNext.Height / 203f;
                    float widthInches = bitmapNext.Width / 203f;

                    var pagePaperSize = new PaperSize("Page " + _pageNum + 1 + " Custom Size", (int)(widthInches * 100), (int)(heightInches * 100));
                    args.PageSettings.PaperSize = pagePaperSize;
                }
            }

            PrintImage(bitmap, args);
        }
    }
}
