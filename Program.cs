
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
    }
}