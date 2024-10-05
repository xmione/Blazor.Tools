using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.SessionManagement.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Blazor.Tools.BlazorBundler.Utilities.Others;
using System.Buffers.Text;
using System.Data;
using System.Net.Http;

namespace Blazor.Tools.BlazorBundler.SessionManagement
{
    /// <summary>
    /// Manages the session data for the application, providing methods to save and retrieve
    /// session variables. It interacts with the session table service to store and retrieve
    /// serialized session data.
    /// Very Important Note: I used Newtonsoft.Json here instead of System.Text.Json because SystemText.Json 
    ///                      at this moment could not handle and doesn't support the serialization and deserialization 
    ///                      of System.Type instances, which are part of the DataColumn definitions in a DataTable. 
    ///                      This limitation is currently present in System.Text.Json. So 1 up to Newtonsoft.
    /// </summary>
    public class SessionManager
    {
        private static Lazy<SessionManager> _instance = new Lazy<SessionManager>(() => new SessionManager());

        private ICommonService<SessionTable, ISessionTable, IReportItem>? _sessionTableService;
        private string? _apiDLLPath;
        private SessionTable _sessionTable;
        private string _selectedFieldValue = string.Empty;
        public static SessionManager Instance => _instance.Value;

        /// <summary>
        /// Constructor to use the Serialize and Deserialize methods
        /// <param name="apiDLLPath">The full path name of the API dll file.</param>
        /// </summary>
        private SessionManager(string? apiDLLPath = null)
        {
            _sessionTable = new SessionTable();
            _sessionTableService = null;
            _apiDLLPath = apiDLLPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager"/> class with the specified session table service.
        /// </summary>
        /// <param name="sessionTableService">The session table service to interact with session data.</param>
        /// <param name="apiDLLPath">The full path name of the API dll file.</param>
        private SessionManager(ICommonService<SessionTable, ISessionTable, IReportItem> sessionTableService, string? apiDLLPath = null)
        {
            _sessionTable = new SessionTable();
            _sessionTableService = sessionTableService;
            _apiDLLPath = apiDLLPath;
        }

        /// <summary>
        /// Public static method to get the instance with a service
        /// </summary> 
        /// <param name="sessionTableService">The session table service to interact with session data.</param>
        /// <returns>SessionManager - The SessionManager class with service instance.</returns>
        public static SessionManager GetInstance(ICommonService<SessionTable, ISessionTable, IReportItem>? sessionTableService = null, string? apiDLLPath = null)
        {
            // Access the singleton instance
            var instance = _instance.Value;
            // If the singleton is already created
            if (_instance.IsValueCreated)
            {
                // If we need to set a service and it hasn't been set yet
                if (sessionTableService != null && instance._sessionTableService == null)
                {
                    instance._sessionTableService = sessionTableService;
                }
                
                // If we need to check the Web API health and it hasn't been set yet
                if (instance._apiDLLPath == null)
                {
                    instance._apiDLLPath = apiDLLPath;
                }
            }
            else
            {
                // If it's the first creation and we have a service to set
                if (sessionTableService != null)
                {
                    _instance = new Lazy<SessionManager>(() => new SessionManager(sessionTableService, apiDLLPath));
                }
            }

            return _instance.Value;
        }

        /// <summary>
        /// Saves the specified value to the session table. Optionally serializes the value if specified.
        /// </summary>
        /// <typeparam name="T">The type of the value to be saved.</typeparam>
        /// <param name="name">The session variable name.</param>
        /// <param name="value">The value to be saved.</param>
        /// <param name="serialize">If true, the value will be serialized before saving.</param>
        /// <returns>A Task representing the asynchronous operation, with a SessionTable object.</returns>
        public async Task<SessionTable> SaveToSessionTableAsync<T>(string name, T value, bool serialize = false)
        {
            try
            {
                if (value == null)
                {
                    return _sessionTable;
                }

                var serializedObject = serialize ? await value.SerializeAsync() : value.ToString() ?? string.Empty;

                if (_sessionTableService != null)
                {
                    await _sessionTableService.RunAPIDLLAsync(_apiDLLPath!);
                    // Check first if Name exists
                    var foundSessionItem = await _sessionTableService.GetByNameAsync(name) as SessionTable ?? default!;
                    var byteArray = System.Text.Encoding.UTF8.GetBytes(serializedObject);
                    _sessionTable = foundSessionItem ?? new SessionTable();
                    _sessionTable.Name = name;
                    _sessionTable.Value = byteArray;
                    _sessionTable.ExpiresAtTime = DateTimeOffset.Now.AddMinutes(5);

                    _sessionTable = await _sessionTableService.SaveAsync(_sessionTable) ?? new SessionTable();
                }
                
                
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            return _sessionTable;
        }

        /// <summary>
        /// Saves the specified value to the session table. Optionally serializes the value if specified.
        /// </summary>
        /// <typeparam name="T">The type of the value to be saved.</typeparam>
        /// <param name="sessionItems">The SessionItem list that contains all te session item variables.</param>
        /// <returns>A Task representing the asynchronous operation, with a SessionTable object.</returns>
        public async Task<SessionTable> SaveToSessionTableAsync<T>(List<SessionItem> sessionItems)
        {
            try
            {
                foreach (var sessionItem in sessionItems)
                {
                    if (sessionItem != null)
                    {
                        string serializedObject = string.Empty;

                        if (sessionItem.Value != null)
                        {
                            serializedObject = sessionItem.Serialize
                                ? (await sessionItem.Value.SerializeAsync() ?? string.Empty)
                                : sessionItem.Value.ToString() ?? string.Empty;
                        }

                        if (_sessionTableService != null)
                        {
                            await _sessionTableService.RunAPIDLLAsync(_apiDLLPath!);
                            // Check first if Name exists
                            var foundSessionItem = await _sessionTableService.GetByNameAsync(sessionItem.Key) ?? default!;
                            var byteArray = System.Text.Encoding.UTF8.GetBytes(serializedObject);
                            _sessionTable = foundSessionItem ?? new SessionTable();
                            _sessionTable.Name = sessionItem?.Key;
                            _sessionTable.Value = byteArray;
                            _sessionTable.ExpiresAtTime = DateTimeOffset.Now.AddMinutes(5);

                            _sessionTable = await _sessionTableService.SaveAsync(_sessionTable) ?? new SessionTable();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            return _sessionTable;
        }

        /// <summary>
        /// Retrieves session variable value from the SessionTable.
        /// It automatically determines if it would deserialize it or not based on the type given.
        /// </summary>
        /// <typeparam name="T">The type given.</typeparam>
        /// <param name="name">The session variable name.</param>
        /// <returns>Task<typeparamref name="T"/>?</returns>
        public async Task<T?> RetrieveFromSessionTableAsync<T>(string name)
        {
            try
            {
                if (_sessionTableService != null)
                {
                    await _sessionTableService.RunAPIDLLAsync(_apiDLLPath!);
                    var sessionTable = await _sessionTableService.GetByNameAsync(name);
                    if (sessionTable != null)
                    {
                        var serializedData = System.Text.Encoding.UTF8.GetString(sessionTable.Value);

                        if (!string.IsNullOrEmpty(serializedData))
                        {
                            if (typeof(T) == typeof(string))
                            {
                                return (T)(object)serializedData;
                            }

                            var deserializedData = await serializedData.DeserializeAsync<T>();
                            return deserializedData;
                        }

                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
                return default;
            }
        }

        /// <summary>
        /// For testing DataRows[], this method saves the DataRows[] to an xml file to be able to used it in the C# Interactive Window.
        /// </summary>
        /// <param name="dataRows">The DataRow[] data to be saved and tested.</param>
        /// <returns>(string) File path of the saved xml file.</returns>
        public static string SaveDataRowsForTestingAsync(IEnumerable<DataRow> dataRows, string tableName)
        {

            // Create a DataTable to hold the DataRow[]
            DataTable dataTable = dataRows.First().Table.Clone(); // Clone the structure of the original DataTable
            dataTable.TableName = tableName;
            foreach (DataRow row in dataRows)
            {
                dataTable.ImportRow(row);
            }

            // Save the DataTable to an XML file
            string filePath = "dataRows.xml";
            dataTable.WriteXml(filePath, XmlWriteMode.WriteSchema);

            return filePath;
        }

        /// <summary>
        /// Reads .xml file and saves it to an IEnumerable<DataRow> object.
        /// </summary>
        /// <param name="filePath">The full Path of the .xml file.</param>
        /// <returns></returns>
        public static IEnumerable<DataRow> ReadDataRowsFromXml(string filePath)
        {
            DataTable dataTable = new DataTable();
            dataTable.ReadXml(filePath);

            foreach (DataRow row in dataTable.Rows)
            {
                yield return row;
            }
        }

        /// <summary>
        /// Retrieves a session list from the Session table.
        /// </summary>
        /// <param name="sessionItems">IList<SessionItem> - refers to the list of session items with or without
        /// default values. It is used to retrieve session variables from the session table.</param>
        /// <returns></returns>
        public async Task<IList<SessionItem>> RetrieveSessionListAsync(IList<SessionItem> sessionItems)
        {
            try
            {
                foreach (var sessionItem in sessionItems)
                {
                    var sessionItemValue = sessionItem.Value;

                    // Get the runtime type of the session item's value
                    Type type = sessionItem.Type;

                    // Retrieve the generic method 'RetrieveFromSessionTableAsync' definition
                    var methodInfo = typeof(SessionManager)
                        .GetMethod(nameof(RetrieveFromSessionTableAsync));

                    // Create a method info object for the constructed generic method with the runtime type
                    var genericMethod = methodInfo?.MakeGenericMethod(type);

                    // Safely access sessionItem.Key if sessionItem is not null
                    var sessionKey = sessionItem?.Key;

                    if (sessionKey != null)
                    {
                        // If sessionKey is not null, attempt to invoke the method and get the result
                        var task = (Task?)genericMethod?.Invoke(this, new object[] { sessionKey });

                        if (task != null)
                        {
                            // Await the task to get the result
                            await task;

                            // Extract the result value (assuming Task<T> as return type)
                            var resultProperty = task.GetType().GetProperty("Result");

                            if (resultProperty != null)
                            {
                                if (sessionItem != null)
                                {
                                    // Assign the result value to sessionItem.Value
                                    var foundSessionItemValue = resultProperty.GetValue(task);

                                    if (foundSessionItemValue == null)
                                    {
                                        sessionItem.Value = sessionItemValue;

                                        // Save session item value immediately to SessionTable
                                        await SaveToSessionTableAsync(sessionKey, sessionItemValue, sessionItem.Serialize);
                                    }
                                    else
                                    {
                                        sessionItem.Value = foundSessionItemValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Handle the case where task is null
                            Console.WriteLine("Warning: Task was null after invoking the generic method.");
                        }
                    }
                    else
                    {
                        // Handle the case where sessionKey is null
                        Console.WriteLine("Warning: sessionKey is null, unable to invoke method.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            return sessionItems;
        }

        /// <summary>
        /// Clears the session table (SessionTable).
        /// </summary>
        public async Task ClearSessionAsync()
        {
            try
            {
                if (_sessionTableService != null)
                {
                    var sessionItems = await _sessionTableService.GetAllAsync();
                    if (sessionItems != null)
                    {
                        foreach (var sessionItem in sessionItems)
                        {
                            await _sessionTableService.DeleteAsync(sessionItem.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            await Task.CompletedTask;
        }

    }

}
