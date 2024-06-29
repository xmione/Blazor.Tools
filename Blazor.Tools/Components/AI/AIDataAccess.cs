using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.ML;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace Blazor.Tools.Components.AI
{
    public class AIDataAccess
    {
        private readonly string connectionString;
        // Event for reporting training progress
        public event Action<string> TrainingProgressUpdated = default!;

        public AIDataAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<IEnumerable<SentimentData>> GetSentimentDataAsync()
        {
            IEnumerable<SentimentData> sentimentData = default!;
            using var connection = new SqlConnection(connectionString);
            sentimentData = await connection.QueryAsync<SentimentData>("SELECT * FROM SentimentData");

            return sentimentData;
        }

        public async Task InsertSentimentDataAsync(SentimentData data)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var sql = "INSERT INTO SentimentData (Sentiment, SentimentText) VALUES (@Sentiment, @SentimentText)";
                await connection.ExecuteAsync(sql, new { data.Sentiment, data.SentimentText });
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error inserting Sentiment data: {ex.Message}");
                throw;
            }
        }

        public async Task InsertSentimentPredictionAsync(SentimentPrediction prediction)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var sql = "INSERT INTO SentimentPredictions (Prediction, Probability, Score) VALUES (@Prediction, @Probability, @Score)";
                await connection.ExecuteAsync(sql, new { prediction.Prediction, prediction.Probability, prediction.Score });
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error inserting Sentiment prediction: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<LanguageData>> GetLanguageDataAsync()
        {
            IEnumerable<LanguageData> languangeData = default!;
            using var connection = new SqlConnection(connectionString);
            languangeData = await connection.QueryAsync<LanguageData>("SELECT * FROM LanguageData");

            return languangeData;
        }

        public async Task InsertLanguageDataAsync(LanguageData data)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var sql = "INSERT INTO LanguageData (Question, Response) VALUES (@Question, @Response)";
                await connection.ExecuteAsync(sql, new {data.Question, data.Response });
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error inserting Language data: {ex.Message}");
                throw;
            }
        }

        public async Task InsertLanguagePredictionAsync(LanguagePrediction prediction)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var sql = "INSERT INTO LanguagePredictions (Response, Probability, Score) VALUES (@Response, @Probability, @Score)";

                // Serialize Score array to JSON
                var scoreJson = JsonConvert.SerializeObject(prediction.Score);

                await connection.ExecuteAsync(sql, new { prediction.Response, prediction.Probability, Score = scoreJson });
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error inserting language prediction: {ex.Message}");
                throw;
            }
        }


        public async Task InsertModelFileAsync(string fileName, byte[] modelData)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO ModelFiles (FileName, ModelData) VALUES (@FileName, @ModelData)";
            await connection.ExecuteAsync(sql, new { FileName = fileName, ModelData = modelData });
        }

        public async Task<byte[]> GetModelFileAsync(string fileName)
        {
            byte[] modelData = Array.Empty<byte>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                var sql = "SELECT ModelData FROM ModelFiles WHERE FileName = @FileName";
                modelData = await connection.QuerySingleOrDefaultAsync<byte[]>(sql, new { FileName = fileName }) ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error retrieving model file '{fileName}' from database: {ex.Message}");
            }

            return modelData;
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
                var words = topic.Split(' ');

                var sql = "SELECT Information FROM GeneralInformation WHERE ";

                for (int i = 0; i < words.Length; i++)
                {
                    if (i > 0)
                        sql += " OR ";

                    sql += "Topic LIKE @Topic" + i;
                }

                var parameters = new DynamicParameters();
                for (int i = 0; i < words.Length; i++)
                {
                    parameters.Add("@Topic" + i, "%" + words[i] + "%");
                }

                generalInformation = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters) ?? string.Empty;
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error getting general information: {ex.Message}");
            }

            return generalInformation;
        }

        public async Task<IEnumerable<LanguageData>> GetLanguageSentimentDataAsync()
        {
            IEnumerable<LanguageData> languageSentimentData = default!;
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM LanguageSentimentData";
            languageSentimentData = await connection.QueryAsync<LanguageData>(sql);

            return languageSentimentData;
        }

        public async Task UploadLanguageSentimentDataAsync(string filePath)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var parser = new TextFieldParser(filePath)
                {
                    TextFieldType = FieldType.Delimited
                };
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields != null && fields.Length == 2)
                    {
                        var question = fields[0];
                        var response = fields[1];

                        var sql = "INSERT INTO LanguageSentimentData (Question, Response) VALUES (@Question, @Response)";
                        await connection.ExecuteAsync(sql, new { Question = question, Response = response });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading language training data: {ex.Message}");
            }
        }

        public async Task TrainSentimentModelAsync(string dataPath)
        {
            try
            {
                var startTime = DateTime.Now;

                var mlContext = new MLContext();
                var data = mlContext.Data.LoadFromTextFile<SentimentData>(dataPath, separatorChar: ',', hasHeader: true);
                var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.SentimentText))
                                            .Append(mlContext.Transforms.CopyColumns("Label", nameof(SentimentData.Sentiment)));

                var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                // Report training progress
                TrainingProgressUpdated?.Invoke("Training Sentiment Model...");

                var trainedModel = trainingPipeline.Fit(data);

                // Save trained model to database
                using var memoryStream = new MemoryStream();
                mlContext.Model.Save(trainedModel, data.Schema, memoryStream);
                var modelFileName = "SentimentAnalysisModel.zip";
                await InsertModelFileAsync(modelFileName, memoryStream.ToArray());

                var trainingDuration = DateTime.Now - startTime;
                TrainingProgressUpdated?.Invoke($"Sentiment model trained successfully! Training duration: {trainingDuration.TotalMinutes:F2} minutes.");
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error during sentiment model training: {ex.Message}");
            }
        }

        public async Task TrainLanguageModelAsync(string dataPath)
        {
            try
            {
                var startTime = DateTime.Now;
                var mlContext = new MLContext();

                // Load data
                var data = mlContext.Data.LoadFromTextFile<LanguageData>(dataPath, separatorChar: ',', hasHeader: true);

                // Define data processing pipeline
                var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", nameof(LanguageData.Question))
                                            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(LanguageData.Response)));

                // Define the model training pipeline
                var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features");

                var trainingPipeline = dataProcessPipeline.Append(trainer)
                                                           .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

                // Report training progress
                TrainingProgressUpdated?.Invoke("Training Language Model...");

                // Train the model
                var trainedModel = trainingPipeline.Fit(data);

                // Save trained model to database
                using var memoryStream = new MemoryStream();
                mlContext.Model.Save(trainedModel, data.Schema, memoryStream);
                var modelFileName = "LanguageAnalysisModel.zip";
                await InsertModelFileAsync(modelFileName, memoryStream.ToArray());

                var trainingDuration = DateTime.Now - startTime;
                TrainingProgressUpdated?.Invoke($"Language model trained successfully! Training duration: {trainingDuration.TotalMinutes:F2} minutes.");
            }
            catch (Exception ex)
            {
                TrainingProgressUpdated?.Invoke($"Error during language model training: {ex.Message}");
            }
        }

        public async Task<string> GetResponseAsync(string input)
        {
            string response = string.Empty;
            input = input.ToLower().Trim();

            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT Response FROM LanguageSentimentData WHERE LOWER(Question) = @Question";
            response = await connection.QuerySingleOrDefaultAsync<string>(sql, new { Question = input }) ?? "I'm sorry, I don't understand that. Can you please rephrase?";

            return response;
        }
    }
}
