namespace Blazor.Tools.BlazorBundler
{
    public class ProcessManager
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public ProcessManager()
        {
            Step1_DownloadFiles().Wait(); // Ensure to call the async method correctly
        }

        private async Task Step1_DownloadFiles()
        {
            // List of files to download from bootstrap-icons
            List<string> files = new List<string>
            {
                "font/bootstrap-icons.css",
                "font/fonts/bootstrap-icons.woff",
                "font/fonts/bootstrap-icons.woff2"
            };

            // Download each file asynchronously
            var tasks = new List<Task>();
            var url = "https://cdn.jsdelivr.net/npm/bootstrap-icons/";
            var bootstrapIconsFolder = "wwwroot/bootstrap-icons/";

            foreach (var file in files)
            {
                tasks.Add(DownloadFileAsync(url + file, Path.Combine(bootstrapIconsFolder, file)));
            }

            await Task.WhenAll(tasks);
        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            try
            {
                // Ensure the destination directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                // Download the file
                using (var response = await httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(destinationPath, content);
                }

                Console.WriteLine($"Downloaded {url} to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download {url}: {ex.Message}");
            }
        }
    }
}
