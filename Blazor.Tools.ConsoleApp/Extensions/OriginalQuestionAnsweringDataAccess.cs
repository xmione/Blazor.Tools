using System.Data.SqlClient;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public class OriginalQuestionAnsweringDataAccess
    {
        private readonly string _connectionString;

        public OriginalQuestionAnsweringDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Create
        public void Create(OriginalQuestionAnsweringData data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var commandText = @"INSERT INTO OriginalQuestionAnsweringData 
                            (Annotations, DocumentHtml, DocumentTitle, DocumentTokens, 
                             DocumentUrl, ExampleId, LongAnswerCandidates, 
                             QuestionText, QuestionTokens, DocumentText) 
                            VALUES 
                            (@Annotations, @DocumentHtml, @DocumentTitle, @DocumentTokens, 
                             @DocumentUrl, @ExampleId, @LongAnswerCandidates, 
                             @QuestionText, @QuestionTokens, @DocumentText)";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Annotations", (object)data.Annotations ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentHtml", (object)data.DocumentHtml ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentTitle", (object)data.DocumentTitle ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentTokens", (object)data.DocumentTokens ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentUrl", (object)data.DocumentUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ExampleId", (object)data.ExampleId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LongAnswerCandidates", (object)data.LongAnswerCandidates ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QuestionText", (object)data.QuestionText ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QuestionTokens", (object)data.QuestionTokens ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentText", (object)data.DocumentText ?? DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


        // Read
        public OriginalQuestionAnsweringData? Read(int id)
        {
            OriginalQuestionAnsweringData? data = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM OriginalQuestionAnsweringData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data =  new OriginalQuestionAnsweringData
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Annotations = reader.GetString(reader.GetOrdinal("Annotations")),
                                DocumentHtml = reader.GetString(reader.GetOrdinal("DocumentHtml")),
                                DocumentTitle = reader.GetString(reader.GetOrdinal("DocumentTitle")),
                                DocumentTokens = reader.GetString(reader.GetOrdinal("DocumentTokens")),
                                DocumentUrl = reader.GetString(reader.GetOrdinal("DocumentUrl")),
                                ExampleId = reader.GetString(reader.GetOrdinal("ExampleId")),
                                LongAnswerCandidates = reader.GetString(reader.GetOrdinal("LongAnswerCandidates")),
                                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                                QuestionTokens = reader.GetString(reader.GetOrdinal("QuestionTokens")),
                                DocumentText = reader.GetString(reader.GetOrdinal("DocumentText")) 
                            };
                        }
                    }
                }
            }

            return data;
        }

        // Update
        public void Update(OriginalQuestionAnsweringData qaData)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OriginalQuestionAnsweringData SET Question = @Question, Context = @Context, Answer = @Answer WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Annotations", qaData.Annotations);
                    command.Parameters.AddWithValue("@DocumentHtml", qaData.DocumentHtml);
                    command.Parameters.AddWithValue("@DocumentTitle", qaData.DocumentTitle);
                    command.Parameters.AddWithValue("@DocumentTokens", qaData.DocumentTokens);
                    command.Parameters.AddWithValue("@DocumentUrl", qaData.DocumentUrl);
                    command.Parameters.AddWithValue("@ExampleId", qaData.ExampleId);
                    command.Parameters.AddWithValue("@LongAnswerCandidates", qaData.LongAnswerCandidates);
                    command.Parameters.AddWithValue("@QuestionText", qaData.QuestionText);
                    command.Parameters.AddWithValue("@QuestionTokens", qaData.QuestionTokens);
                    command.Parameters.AddWithValue("@DocumentText", qaData.DocumentText);
                    command.Parameters.AddWithValue("@ID", qaData.ID);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete
        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM OriginalQuestionAnsweringData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // List All
        public List<OriginalQuestionAnsweringData> ListAll()
        {
            var dataList = new List<OriginalQuestionAnsweringData>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM OriginalQuestionAnsweringData", connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader != null && reader.Read())
                    {
                        var data = new OriginalQuestionAnsweringData
                        {
                            ID = reader.GetInt32(0),
                            Annotations = reader.GetString(1),
                            DocumentHtml = reader.IsDBNull(2) ? default! : reader.GetString(2),
                            DocumentTitle = reader.IsDBNull(3) ? default! : reader.GetString(3),
                            DocumentTokens = reader.IsDBNull(4) ? default! : reader.GetString(4),
                            DocumentUrl = reader.IsDBNull(5) ? default! : reader.GetString(5),
                            ExampleId = reader.GetString(6),
                            LongAnswerCandidates = reader.IsDBNull(7) ? default! : reader.GetString(7),
                            QuestionText = reader.GetString(8),
                            QuestionTokens = reader.IsDBNull(9) ? default! : reader.GetString(9),
                            DocumentText = reader.IsDBNull(10) ? default! : reader.GetString(10)
                        };

                        dataList.Add(data);
                    }
                }
            }

            return dataList;
        }

        // List All with Pagination
        public List<OriginalQuestionAnsweringData> ListAll(int pageSize, int pageNumber)
        {
            var dataList = new List<OriginalQuestionAnsweringData>();

            using (var connection = new SqlConnection(_connectionString))
            {
                // Calculate offset based on pageNumber and pageSize
                int offset = (pageNumber - 1) * pageSize;

                var commandText = @"SELECT * FROM OriginalQuestionAnsweringData 
                            ORDER BY ID 
                            OFFSET @Offset ROWS 
                            FETCH NEXT @PageSize ROWS ONLY";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Offset", offset);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = new OriginalQuestionAnsweringData
                            {
                                ID = reader.GetInt32(0),
                                Annotations = reader.GetString(1),
                                DocumentHtml = reader.IsDBNull(2) ? default! : reader.GetString(2),
                                DocumentTitle = reader.IsDBNull(3) ? default! : reader.GetString(3),
                                DocumentTokens = reader.IsDBNull(4) ? default! : reader.GetString(4),
                                DocumentUrl = reader.IsDBNull(5) ? default! : reader.GetString(5),
                                ExampleId = reader.GetString(6),
                                LongAnswerCandidates = reader.IsDBNull(7) ? default! : reader.GetString(7),
                                QuestionText = reader.GetString(8),
                                QuestionTokens = reader.IsDBNull(9) ? default! : reader.GetString(9),
                                DocumentText = reader.IsDBNull(10) ? default! : reader.GetString(10)
                            };

                            dataList.Add(data);
                        }
                    }
                }
            }

            return dataList;
        }

        public int GetTotalRecordCount()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var commandText = "SELECT COUNT(*) FROM OriginalQuestionAnsweringData";

                using (var command = new SqlCommand(commandText, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }

    }

}
