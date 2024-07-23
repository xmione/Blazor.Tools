using HtmlAgilityPack;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class WebScraper
    {
        public void ScrapeNewsHeadlines()
        {
            var web = new HtmlWeb();
            var doc = web.Load("https://example.com/news");

            var headlines = doc.DocumentNode
                               .SelectNodes("//h2[@class='headline']")
                               .Select(node => node.InnerText.Trim())
                               .ToList();

            foreach (var headline in headlines)
            {
                Console.WriteLine(headline);
            }
        }
    }
}
