
namespace FetchHTTPResource
{
    internal class Program
    {
        static readonly HttpClient httpClient = new HttpClient();
        static async Task Main(string[] args)
        {

            var clientWrapper = new HTTPClientWrapper(httpClient);
            string pageBody = await clientWrapper.GetString("https://example.com");
            Console.WriteLine(pageBody);

            string pageBody2 = await clientWrapper.GetString("https://exampleeee.com");
            Console.WriteLine(pageBody2);
        }
    }


    internal class HTTPClientWrapper
    {
        private HttpClient client;

        public HTTPClientWrapper(HttpClient client)
        {
            this.client = client;
        }

        public async Task<string> GetString(string uri)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // above three commends can be replaced with method below:
                // string responseBody = await client.GetStringAsync(url);

                return responseBody;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException occured:");
                Console.WriteLine("Message: {0} ", ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Fetch resource and save it to destination.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="destination">Destination path indicates where file should be saved. If it ends with directory separator then file will be saved into that directory and name fill be implied. 
        /// <para>If null use current working directory.</para></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <returns>Saved file path.</returns>
        public async Task<string> GetFile(string uri, string? destination = null)
        {
            ArgumentNullException.ThrowIfNull(uri);

            bool doImplyName = false;
            if (String.IsNullOrEmpty(destination))
            {
                destination = Directory.GetCurrentDirectory();
                doImplyName = true;
            }

            /* In below method invocation parameter `destination` is ReadOnlySpan<char>.
             * Span types family make it quick to access parts of contigous memory without using unsafe pointers and copying data whether it's on heap or stack.
             * see: https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay
             */
            doImplyName |= Path.EndsInDirectorySeparator(destination);
            string? filePath = null;
            if (doImplyName)
            {
                bool directoryExists = Directory.Exists(destination);
                if (!directoryExists)
                {
                    Directory.CreateDirectory(destination);
                }
            }

            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            HttpContent content = response.Content;

            if (doImplyName)
            {
                // Infer file name from Content-Location or Target URI.
                IEnumerable<string>? contentLocations;
                response.Headers.TryGetValues("Content-Location", out contentLocations);
                bool oneProcessed = false;
                string? name = null;
                if (contentLocations is not null)
                {
                    foreach (string value in contentLocations)
                    {
                        if (oneProcessed)
                        {
                            throw new Exception("More than one Content-Location value found.");
                        }

                        name = value.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
                        oneProcessed = true;
                    }
                }

                name ??= uri.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();

                filePath = Path.Combine(destination, name);
            }
            filePath ??= destination;
            FileStream fileStream = File.Create(filePath);
            Stream stream = content.ReadAsStream();
            stream.CopyTo(fileStream);
            fileStream.Flush();
            return filePath;
        }
    }
}