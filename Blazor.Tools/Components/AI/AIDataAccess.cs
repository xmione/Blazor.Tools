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
    }
}
