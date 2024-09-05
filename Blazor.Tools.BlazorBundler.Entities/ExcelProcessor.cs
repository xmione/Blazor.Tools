using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using ClosedXML.Excel;
using DataTable = System.Data.DataTable;
namespace Blazor.Tools.BlazorBundler.Entities
{
    public class ExcelProcessor
    {
        private readonly string _connectionString;

        public ExcelProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ProcessExcelFileAsync(string filePath)
        {
            var dataSet = await ReadExcelDataAsync(filePath);
            await SaveDataToDatabaseAsync(dataSet);
        }

        public async Task<DataSet> ReadExcelDataAsync(string filePath)
        {
            var dataSet = new DataSet();
            var excelConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0 Xml;HDR=NO;'";

            try
            {
                var sheetNames = await GetSheetNamesAsync(filePath);

                using (var connection = new OleDbConnection(excelConnectionString))
                {
                    await connection.OpenAsync();

                    foreach (var sheetName in sheetNames)
                    {
                        var targetSheetName = "S_" + await SanitizeSheetNameAsync(sheetName);

                        Console.WriteLine("targetSheetName: {0} sheetName: {1}", targetSheetName, sheetName);

                        var dataTable = new DataTable(targetSheetName);
                        var query = $"SELECT * FROM [{sheetName}$]";
                        using (var dataAdapter = new OleDbDataAdapter(query, connection))
                        {
                            await Task.Run(() => dataAdapter.Fill(dataTable));
                        }

                        dataSet.Tables.Add(dataTable);
                    }
                }
            }
            catch (OleDbException ex)
            {
                // Handle OleDbException
                Console.WriteLine($"Error reading Excel data: {ex.Message}");
                // You can log the error, display a user-friendly message, or perform any other necessary action
            }

            await Task.CompletedTask;
            return dataSet;
        }

        public Task<List<string>> GetSheetNamesAsync(string filePath)
        {
            var sheetNames = new List<string>();

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    sheetNames.Add(worksheet.Name);
                }
            }

            return Task.FromResult(sheetNames);
        }

        private async Task<string> SanitizeSheetNameAsync(string sheetName)
        {
            // Replace invalid characters with underscores
            var invalidChars = new[] { ' ', '#', ',', '.', ':', '$', '-', '&' };
            foreach (var invalidChar in invalidChars)
            {
                sheetName = sheetName.Replace(invalidChar, '_');
            }

            await Task.CompletedTask;
            return sheetName;
        }

        public async Task SaveDataToDatabaseAsync(DataSet data)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (DataTable table in data.Tables)
            {
                var createTableCommand = await GenerateCreateTableCommandAsync(table);
                using var createTableCmd = new SqlCommand(createTableCommand, connection);
                await createTableCmd.ExecuteNonQueryAsync();

                foreach (DataRow row in table.Rows)
                {
                    var insertCommand = await GenerateInsertCommandAsync(table, row);
                    using var insertCmd = new SqlCommand(insertCommand, connection);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await Task.CompletedTask;
        }

        private async Task<string> GenerateCreateTableCommandAsync(DataTable table)
        {
            var commandText = $"IF OBJECT_ID('{table.TableName}', 'U') IS NOT NULL DROP TABLE {table.TableName}; CREATE TABLE {table.TableName} (";
            foreach (DataColumn column in table.Columns)
            {
                commandText += $"[{column.ColumnName}] NVARCHAR(MAX),";
            }
            commandText = commandText.TrimEnd(',') + ")";

            await Task.CompletedTask;
            return commandText;
        }

        private async Task<string> GenerateInsertCommandAsync(DataTable table, DataRow row)
        {
            var columns = string.Join(",", table.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]"));
            var values = string.Join(",", row.ItemArray.Select(v => $"'{v.ToString().Replace("'", "''")}'"));
            var commandText = $"INSERT INTO {table.TableName} ({columns}) VALUES ({values})";

            await Task.CompletedTask;
            return commandText;
        }
    }

}
