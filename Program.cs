
namespace FetchHTTPResource
{
    internal class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {

            string pageBody = await FetchPage("https://example.com");
            Console.WriteLine(pageBody);

            string pageBody2 = await FetchPage("https://exampleeee.com");
            Console.WriteLine(pageBody2);
        }

        static async Task<string> FetchPage(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
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