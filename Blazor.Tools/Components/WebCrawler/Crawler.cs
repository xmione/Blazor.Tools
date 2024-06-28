namespace Blazor.Tools.Components.WebCrawler
{
    using System;
    using System.Net.Http;
    using HtmlAgilityPack;

    public class Crawler
    {
        private readonly HttpClient _httpClient;

        public Crawler()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> FetchWebPageAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching {url}: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<string> ExtractLinksFromHtml(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var links = doc.DocumentNode.SelectNodes("//a[@href]")
                        .Select(link => link.GetAttributeValue("href", string.Empty))
                        .Where(href => !string.IsNullOrEmpty(href));

            return links;
        }

        // Add more methods as needed for specific scraping tasks
    }

}
