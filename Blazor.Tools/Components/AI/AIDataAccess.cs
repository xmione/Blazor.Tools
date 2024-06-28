using Microsoft.Data.SqlClient;
using Dapper;
namespace Blazor.Tools.Components.AI
{
    public class AIDataAccess
    {
        private readonly string connectionString;

        public AIDataAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<IEnumerable<TrainingData>> GetTrainingDataAsync()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TrainingData>("SELECT * FROM TrainingData");
        }

        public async Task InsertTrainingDataAsync(TrainingData data)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO TrainingData (SentimentText, Sentiment) VALUES (@SentimentText, @Sentiment)";
            await connection.ExecuteAsync(sql, data);
        }

        public async Task InsertModelFileAsync(string fileName, byte[] modelData)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO ModelFiles (FileName, ModelData) VALUES (@FileName, @ModelData)";
            await connection.ExecuteAsync(sql, new { FileName = fileName, ModelData = modelData });
        }

        public async Task<byte[]> GetModelFileAsync(string fileName)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT ModelData FROM ModelFiles WHERE FileName = @FileName";
            return await connection.QuerySingleOrDefaultAsync<byte[]>(sql, new { FileName = fileName });
        }

        public async Task InsertGeneralInformationAsync(string topic, string information)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO GeneralInformation (Topic, Information) VALUES (@Topic, @Information)";
            await connection.ExecuteAsync(sql, new { Topic = topic, Information = information });
        }

        public async Task<string> GetGeneralInformationAsync(string topic)
        { 
            string generalInformation = string.Empty;
            try 
            {
                using var connection = new SqlConnection(connectionString);

                // Split the topic into words
                var words = topic.Split(' ');

                // Query to find matching information
                var sql = "SELECT Information FROM GeneralInformation WHERE ";

                // Build conditions for each word
                for (int i = 0; i < words.Length; i++)
                {
                    if (i > 0)
                        sql += " OR ";

                    sql += "Topic LIKE @Topic" + i;
                }

                // Execute query with parameters
                var parameters = new DynamicParameters();
                for (int i = 0; i < words.Length; i++)
                {
                    parameters.Add("@Topic" + i, "%" + words[i] + "%");
                }

                generalInformation = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters) ?? string.Empty; 
            } 
            catch (Exception ex) 
            {
                Console.WriteLine("Error: {0}", ex.Message); 
            }
            
            return generalInformation;
        }

    }

}
