/*====================================================================================================
    Class Name  : CustomLoggingHandler
    Created By  : Solomio S. Sisante
    Created On  : October 5, 2024
    Purpose     : To manage the custom logging of exceptions.
  ====================================================================================================*/
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Utilities.Exceptions
{
    public class CustomLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Log request details
                AppLogger.WriteInfo($"Sending HTTP request to {request.RequestUri}");
                AppLogger.WriteInfo($"Request method: {request.Method}");
                AppLogger.WriteInfo($"Request headers: {string.Join(", ", request.Headers)}");

                // Send the HTTP request
                var response = await base.SendAsync(request, cancellationToken);

                // Log response details
                AppLogger.WriteInfo($"Received HTTP response from {request.RequestUri}");
                AppLogger.WriteInfo($"Response status code: {response.StatusCode}");
                AppLogger.WriteInfo($"Response headers: {string.Join(", ", response.Headers)}");

                return response;
            }
            catch (HttpRequestException ex)
            {
                // Handle and log errors using AppLogger
                AppLogger.HandleError(ex, $"An error occurred while sending HTTP request to {request.RequestUri}");
                throw;
            }
            catch (TaskCanceledException taskEx) when (cancellationToken.IsCancellationRequested)
            {
                AppLogger.HandleError(taskEx); // Handle cancellation specifically
                throw;
            }
            catch (TaskCanceledException taskEx)
            {
                // Handle timeout if not cancelled
                AppLogger.HandleError(taskEx);
                throw;
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
                throw;
            }
        }
    }

}
