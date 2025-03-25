using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SWIftMSGPRCS.Func2
{
    internal class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public List<MessageRecord> GetMessagesToProcess(SqlConnection connection)
        {
            List<MessageRecord> messages = new List<MessageRecord>();

            string query = "SELECT IdKey, UserReference, MessageContent FROM dbo.Messages WHERE MesgStatus = 'MesgInserted'";
            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    messages.Add(new MessageRecord
                    {
                        IdKey = (int)reader["IdKey"],
                        UserReference = reader["UserReference"]?.ToString(),
                        MessageContent = reader["MessageContent"]?.ToString()
                    });
                }
            }
            return messages;
        }

        public string GetMappingForUserReference(string userReference, SqlConnection connection)
        {
            string query = "SELECT MapValue FROM dbo.MappingUserReferences WHERE MapKey = @UserReference";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserReference", userReference);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["MapValue"]?.ToString();
                    }
                }
            }
            return null;
        }

        public void UpdateMessageStatus(int messageId, string newStatus, SqlConnection connection)
        {
            string query = "UPDATE dbo.Messages SET MesgStatus = @NewStatus WHERE IdKey = @MessageId";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewStatus", newStatus);
                command.Parameters.AddWithValue("@MessageId", messageId);
                command.ExecuteNonQuery();
            }
        }
    }

    internal class MessageRecord
    {
        public int IdKey { get; set; }
        public string UserReference { get; set; }
        public string MessageContent { get; set; }
    }
}
