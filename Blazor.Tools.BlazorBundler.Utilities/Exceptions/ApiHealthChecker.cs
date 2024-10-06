/*====================================================================================================
    Class Name  : ApiHealthChecker
    Created By  : Solomio S. Sisante
    Created On  : October 5, 2024
    Purpose     : To help check the health of a Web API Endpoint.
  ====================================================================================================*/

using static System.Net.WebRequestMethods;

namespace Blazor.Tools.BlazorBundler.Utilities.Exceptions
{
    public class ApiHealthChecker
    {
        private readonly HttpClient _httpClient;

        public ApiHealthChecker(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsApiUpAsync(CancellationToken cancellationToken)
        {
            try
            {
                var healthCheckEndpoint = new Uri(_httpClient.BaseAddress!, "health");

                // Cancel if requested
                cancellationToken.ThrowIfCancellationRequested();

                // Send the request
                var response = await _httpClient.GetAsync(healthCheckEndpoint, cancellationToken);

                await Task.Delay(500);

                // Check if the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API response was not successful: {response.StatusCode}");
                }

                // Return true if the response is successful
                return true;
            }
            catch (Exception ex)
            {
                // Log the error and return false
                AppLogger.HandleError(ex);
                return false;
            }
        }
    }
}
