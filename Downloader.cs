using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrinterService
{
    public class Downloader
    {
        private readonly IDictionary<string, string> _cookies = new Dictionary<string, string>();

        public void AddCookie(string key, string value)
        {
            _cookies.Add(key, value);
        }

        public Task<byte[]> DownloadUrl(string url)
        {
            var task = new FlurlClient(url)
                .WithCookies(_cookies)
                .GetBytesAsync();

            return task;
        }
    }
}
