using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.Converters;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.SessionManagement.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Blazor.Tools.BlazorBundler.Utilities.Others;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using Newtonsoft.Json;
using Polly;
using System.Buffers.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;

namespace Blazor.Tools.BlazorBundler.SessionManagement
{
    public class SessionTableService : ISessionTableService
    {
        private readonly HttpClient _httpClient;
        private bool _isAPIup;
        private ApiHealthChecker? _apiHealthChecker;
        private const int TIMEOUT = 12000;
        public bool IsAPIUp
        {
            get { return _isAPIup; }    
        }

        public SessionTableService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiHealthChecker = new ApiHealthChecker(_httpClient);

        }

        public async Task<IEnumerable<SessionTable>?> GetAllAsync()
        {
            IEnumerable<SessionTable>? list = null;

            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                list = await _httpClient.GetFromJsonAsync<IEnumerable<SessionTable>>("SessionTables/GetAllAsync");
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            return list;
        }

        public async Task<SessionTable?> GetAsync(int id)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var getTask = _httpClient.PostAsJsonAsync("SessionTables/GetAsync", id);
            SessionTable? sessionTable = null;
            try
            {

                getTask.Wait();
                HttpResponseMessage response = getTask.Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    sessionTable = await response.Content.ReadFromJsonAsync<SessionTable>();
                }

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
            return sessionTable;
        }

        public async Task<SessionTable?> GetByNameAsync(string name)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var getTask = _httpClient.PostAsJsonAsync("SessionTables/GetByNameAsync", name);
            SessionTable? sessionTable = null;
            try
            {

                getTask.Wait();
                HttpResponseMessage response = getTask.Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    sessionTable = await response.Content.ReadFromJsonAsync<SessionTable>();
                }

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
            return sessionTable;
        }

        public async Task DeleteAsync(int id)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var getTask = _httpClient.PostAsJsonAsync("SessionTables/DeleteAsync", id);

            try
            {

                await getTask;
                var response = getTask.Result;

                if (response == null)
                {
                    throw new Exception("There was an error in deleting the data.");
                }

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }

        public async Task UploadTableListAsync(List<TargetTable> model)
        {
            if (model == null)
            {
                throw new Exception("Invalid or empty model.");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = new List<JsonConverter> { new DataTableJsonConverter(), new TargetTableColumnConverter() }
                };

                var jsonContent = JsonConvert.SerializeObject(model, settings);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("SessionTables/UploadTableListAsync", content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    AppLogger.WriteInfo($"HTTP Request error. Status Code: {response.StatusCode}. Content: {responseContent}");
                    throw new HttpRequestException($"HTTP Request error. Status Code: {response.StatusCode}");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException httpEx)
            {
                AppLogger.HandleError(httpEx, $"HTTP Request error: {httpEx.Message}");
                if (httpEx.StatusCode.HasValue)
                {
                    AppLogger.HandleError(httpEx, $"HTTP Status Code: {httpEx.StatusCode.Value}");
                }
                throw;
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
                throw;
            }
        }

        public async Task<SessionTable?> SaveDataAsync(ISessionTable? model)
        {
            if (model == null)
            {
                throw new Exception("Invalid or empty model.");
            }

            SessionTable? postedModel = null;

            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var getTask = _httpClient.PostAsJsonAsync("SessionTables/SaveAsync", model);

                getTask.Wait();
                HttpResponseMessage response = getTask.Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    postedModel = await response.Content.ReadFromJsonAsync<SessionTable>();
                }

            }
            catch (HttpRequestException httpEx)
            {
                // Log details of the HTTP request error
                AppLogger.HandleError(httpEx);
                if (httpEx.StatusCode.HasValue)
                {
                    AppLogger.HandleError(httpEx, $"HTTP Status Code: {httpEx.StatusCode.Value}");
                }
                throw;
            }
            catch (Exception ex)
            {
                // Log other exceptions
                AppLogger.HandleError(ex);
                throw;
            }

            return postedModel;
        }

        public async Task<SessionTable?> SaveAsync(SessionTable? model)
        {
            var iModel = await SaveDataAsync(model);

            return iModel;
        }

        public async Task<SessionTable?> SaveAsync(ISessionTable? model)
        {
            var iModel = await SaveDataAsync(model);

            return iModel;
        }

        private async Task<bool> CheckApiHealthAsync(CancellationToken cancellationToken)
        {
            return await _apiHealthChecker?.IsApiUpAsync(cancellationToken)!;
        }

        public async Task RunAPIDLLAsync(string apiDLLPath)
        {
            // Initialize API health status
            CancellationTokenSource cts = new CancellationTokenSource(TIMEOUT);
            _isAPIup = await CheckApiHealthAsync(cts.Token);

            if (_isAPIup)
            {
                return;
            }

            await Task.Run(async () =>
            {
                //["Arguments"] = @"C:\Hermie\AccSol\AccSol.API\bin\Debug\net8.0\AccSol.API.dll ""C:\Hermie\AccSol\AccSol.API\""",
                //var command = "dotnet";
                //var arguments = $"""{apiDLLPath}"" ""{Path.GetDirectoryName(apiDLLPath)}""";

                // Set environment variables within the cmd context
                var setEnvVars = $"set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=https://localhost:7040&&";
                var command = "cmd.exe";
                var arguments = $"/k {setEnvVars} dotnet \"{apiDLLPath}\" \"{Path.GetDirectoryName(apiDLLPath)}\"";

                var commandRunnerArgs = new Dictionary<string, object>()
                {
                    ["FileName"] = command,
                    ["Arguments"] = arguments,
                    ["RedirectStandardOutput"] = false,
                    ["RedirectStandardError"] = false,
                    ["UseShellExecute"] = true,
                    ["CreateNoWindow"] = false,
                };
                var cm = new CommandRunner("RunWebAPI");
                await cm.RunDotnetCommandAsync(commandRunnerArgs, TIMEOUT);

            });

            await Task.Run(async () =>
            {
                while (apiDLLPath != null && !apiDLLPath.IsFileInUse())
                {
                    // Wait for it to be in use and log it
                    AppLogger.WriteInfo($"Waiting to load the Web API file {apiDLLPath}...");
                }

                while (!_isAPIup)
                {
                    // Wait for the Web API to be up and log it
                    AppLogger.WriteInfo($"Waiting for the Web API to start from file {apiDLLPath}...");
                    _isAPIup = await CheckApiHealthAsync(cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                }

                AppLogger.WriteInfo($"Web API has started successfully.");
            });
        }
    }
}
