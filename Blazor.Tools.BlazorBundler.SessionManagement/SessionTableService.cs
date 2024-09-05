using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.SessionManagement.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Blazor.Tools.BlazorBundler.SessionManagement
{
    public class SessionTableService : ISessionTableService
    {
        private readonly HttpClient _httpClient;
        public SessionTableService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
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
                    Console.WriteLine($"HTTP Request error. Status Code: {response.StatusCode}. Content: {responseContent}");
                    throw new HttpRequestException($"HTTP Request error. Status Code: {response.StatusCode}");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
                if (httpEx.StatusCode.HasValue)
                {
                    Console.WriteLine($"HTTP Status Code: {httpEx.StatusCode.Value}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
                if (httpEx.StatusCode.HasValue)
                {
                    Console.WriteLine($"HTTP Status Code: {httpEx.StatusCode.Value}");
                }
                throw;
            }
            catch (Exception ex)
            {
                // Log other exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
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

    }
}
