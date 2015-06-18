using Topshelf;

namespace PrinterService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<PrinterService>(sc =>
                {
                    sc.ConstructUsing(() => new PrinterService());
                    
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });
            });
        }
    }
}

