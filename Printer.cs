using Ghostscript.NET.Rasterizer;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace PrinterService
{
    public class Printer
    {
        private readonly PrintDocument _printer;

        [Obsolete("Find a way to pass this in the event")]
        private Bitmap _currentImage;

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

            _printer.PrintPage += (printer, args) =>
            {
                PrintImage(_currentImage, args);
            };
        }

        /// <summary>
        /// Prints a PDF document.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public void PrintPdf(byte[] document)
        {
            var preparedDocument = PreparePdfForPrinting(document);
            var pages = RasterizePdf(preparedDocument, 203);
            
            foreach (var page in pages)
            {
                PrintSingleImage(page);
            }
        }

        #region Private Methods
        /// <summary>
        /// Rasterizes the PDF document into images.
        /// </summary>
        /// <param name="document">The data.</param>
        /// <returns></returns>
        private static IEnumerable<Bitmap> RasterizePdf(byte[] document, int dpi)
        {
            var pages = new List<Bitmap>();

            using (var rasterizer = new GhostscriptRasterizer())
            {
                using (var ms = new MemoryStream(document))
                {
                    rasterizer.Open(ms);

                    int numPages = rasterizer.PageCount;
                    for (int pageNumber = 1; pageNumber <= numPages; pageNumber++)
                    {
                        var img = rasterizer.GetPage(dpi, dpi, pageNumber);
                        pages.Add(new Bitmap(img));
                    }
                }
            }

            return pages;
        }

        /// <summary>
        /// Prepares the PDF for printing on the thermal printer
        /// 
        /// All this does is crop every page to a tigher bounding box
        /// The bounding box is calcuated using TextMarginFinder
        /// 
        /// This is nessesary because Report Services cannot produce different
        /// page size for each page. Eventually this step will be done on the server
        /// and the server will send a PDF that is already prepared
        /// </summary>
        /// <param name="pdfData">The PDF.</param>
        /// <returns></returns>
        private static byte[] PreparePdfForPrinting(byte[] pdfData)
        {
            using (var memoryStream = new MemoryStream())
            {
                var reader = new PdfReader(pdfData);
                var parser = new PdfReaderContentParser(reader);

                var n = reader.NumberOfPages;
                for (var i = 1; i <= n; i++)
                {
                    var finder = parser.ProcessContent(i, new TextMarginFinder());
                    var cropBox = reader.GetCropBox(i);

                    var margin = finder.GetLlx();

                    cropBox.Bottom = finder.GetLly() - margin;
                    reader.GetPageN(i).Put(PdfName.CROPBOX, new PdfRectangle(cropBox));
                }
                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Prints the single image.
        /// </summary>
        /// <param name="image">The image.</param>
        private void PrintSingleImage(Bitmap image)
        {
            _currentImage = image;

            Console.Write("Printing Single Page... ");

            var heightInches = image.Height / 203f;
            var widthInches = image.Width / 203f;

            var pagePaperSize = new PaperSize("Page Custom Size", (int)(widthInches * 100), (int)(heightInches * 100));
            _printer.DefaultPageSettings.PaperSize = pagePaperSize;
            _printer.Print();
            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Prints the image. Gets called by the PrintDocument PrintPage event
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="args">The <see cref="PrintPageEventArgs"/> instance containing the event data.</param>
        private void PrintImage(Bitmap image, PrintPageEventArgs args)
        {
            Graphics g = args.Graphics;

            float aspectRatio = image.Height / (float)image.Width;
            float height = g.VisibleClipBounds.Width * aspectRatio;
            float width = g.VisibleClipBounds.Width;

            g.DrawImage(image, 0.0f, 0.0f, width, height);

            // draw a small line at the *VERY* bottom of the image
            // so the thermal printer scrolls to it
            // white space at the end of the printed page causes issues
            g.DrawLine(new Pen(Color.Gray), 0, height, width / 10, height);
        }
        #endregion
    }
}
