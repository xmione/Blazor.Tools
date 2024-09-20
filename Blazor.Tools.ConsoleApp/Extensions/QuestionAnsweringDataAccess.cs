using System.Data.SqlClient;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public class QuestionAnsweringDataAccess
    {
        private readonly string _connectionString;

        public QuestionAnsweringDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Create
        public void Create(QuestionAnsweringData qaData)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO QuestionAnsweringData (Question, Context, Answer) VALUES (@Question, @Context, @Answer)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Question", qaData.Question ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Context", qaData.Context ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Answer", qaData.Answer ?? (object)DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Read
        public QuestionAnsweringData? Read(int id)
        {
            QuestionAnsweringData? data = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM QuestionAnsweringData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data = new QuestionAnsweringData
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Question = reader.GetString(reader.GetOrdinal("Question")),
                                Context = reader.GetString(reader.GetOrdinal("Context")),
                                Answer = reader.GetString(reader.GetOrdinal("Answer"))
                            };
                        }
                    }
                }
            }

            return data;
        }

        // Update
        public void Update(QuestionAnsweringData qaData)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE QuestionAnsweringData SET Question = @Question, Context = @Context, Answer = @Answer WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Question", qaData.Question ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Context", qaData.Context ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Answer", qaData.Answer ?? (object)DBNull.Value);
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
                string query = "DELETE FROM QuestionAnsweringData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // List All
        public List<QuestionAnsweringData> ListAll()
        {
            List<QuestionAnsweringData> list = new List<QuestionAnsweringData>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM QuestionAnsweringData";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new QuestionAnsweringData
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Question = reader.GetString(reader.GetOrdinal("Question")),
                                Context = reader.GetString(reader.GetOrdinal("Context")),
                                Answer = reader.GetString(reader.GetOrdinal("Answer"))
                            });
                        }
                    }
                }
            }

            return list;
        }
    }

}
