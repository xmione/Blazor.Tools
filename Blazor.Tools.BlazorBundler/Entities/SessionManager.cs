﻿using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Newtonsoft.Json;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Entities
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
        private readonly ICommonService<SessionTable, ISessionTable, IReportItem> _sessionTableService;
        private SessionTable _sessionTable;
        private string _selectedFieldValue = string.Empty;

        /// <summary>
        /// Constructor to use the Serialize and Deserialize methods
        /// </summary>
        public SessionManager()
        {
            _sessionTableService = default!;
            _sessionTable = default!;
            _selectedFieldValue = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager"/> class with the specified session table service.
        /// </summary>
        /// <param name="sessionTableService">The session table service to interact with session data.</param>
        public SessionManager(ICommonService<SessionTable, ISessionTable, IReportItem> sessionTableService)
        {
            _sessionTable = new SessionTable();
            _sessionTableService = sessionTableService;
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

                // Check first if Name exists
                var foundSessionItem = await _sessionTableService.GetByNameAsync(name) as SessionTable;
                var byteArray = System.Text.Encoding.UTF8.GetBytes(serializedObject);
                _sessionTable = foundSessionItem ?? new SessionTable();
                _sessionTable.Name = name;
                _sessionTable.Value = byteArray;
                _sessionTable.ExpiresAtTime = DateTimeOffset.Now.AddMinutes(5);

                _sessionTable = await _sessionTableService.SaveAsync(_sessionTable) ?? new SessionTable();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
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
    }
}
