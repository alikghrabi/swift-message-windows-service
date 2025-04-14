using System;
using System.Data.SqlClient;
using SWIftMSGPRCS.Func1;

namespace SWIftMSGPRCS.Func1
{
    internal class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int InsertMessageFile(string content)
        {
            int fileId = 0;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO MessageFiles (FileContent, FileDatetimeInsertion, FileCreationDatetime) OUTPUT INSERTED.IdKey VALUES (@Content, @InsertionTime, @CreationTime)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Content", content);
                cmd.Parameters.AddWithValue("@InsertionTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@CreationTime", DateTime.Now);
                conn.Open();
                fileId = (int)cmd.ExecuteScalar();
            }
            return fileId;
        }

        public int InsertMessage(int fileId, string messageContent)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "INSERT INTO Messages (FileId, DateTimeInsertion, MessageContent, OwnBIC, CorBIC, MessageType, SubFormat, UserReference, SLA, UETR, Reference, OrderingCust, BeneficiaryCust, DetailsOfCharges, Block1, Block2, Block3, Block4, MesgStatus) " +
                                   "OUTPUT INSERTED.IdKey " +
                                   "VALUES (@FileId, @InsertionTime, @Content, @OwnBIC, @CorBIC, @MessageType, @SubFormat, @UserReference, @SLA, @UETR, @Reference, @OrderingCust, @BeneficiaryCust, @DetailsOfCharges, @Block1, @Block2, @Block3, @Block4, 'MesgInserted')";

                    SqlCommand cmd = new SqlCommand(query, conn, transaction);
                    cmd.Parameters.AddWithValue("@FileId", fileId);
                    cmd.Parameters.AddWithValue("@InsertionTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Content", messageContent);
                    cmd.Parameters.AddWithValue("@OwnBIC", SwiftMessageExtractor.ExtractOwnBIC(messageContent));
                    cmd.Parameters.AddWithValue("@CorBIC", SwiftMessageExtractor.ExtractCorBIC(messageContent));
                    cmd.Parameters.AddWithValue("@MessageType", SwiftMessageExtractor.ExtractMessageType(messageContent));
                    cmd.Parameters.AddWithValue("@SubFormat", SwiftMessageExtractor.ExtractSubFormat(messageContent));
                    cmd.Parameters.AddWithValue("@UserReference", SwiftMessageExtractor.ExtractField(messageContent, "{108:", "}", "DEFAULT"));
                    cmd.Parameters.AddWithValue("@SLA", SwiftMessageExtractor.ExtractField(messageContent, "{111:", "}", "DEFAULT"));
                    cmd.Parameters.AddWithValue("@UETR", SwiftMessageExtractor.ExtractField(messageContent, "{121:", "}", "DEFAULT"));
                    cmd.Parameters.AddWithValue("@Reference", SwiftMessageExtractor.ExtractField(messageContent, ":20:", "\n", "DEFAULT"));
                    cmd.Parameters.AddWithValue("@OrderingCust", SwiftMessageExtractor.ExtractMultiLineField(messageContent, ":50K:"));
                    cmd.Parameters.AddWithValue("@BeneficiaryCust", SwiftMessageExtractor.ExtractMultiLineField(messageContent, ":59:"));
                    cmd.Parameters.AddWithValue("@DetailsOfCharges", SwiftMessageExtractor.ExtractField(messageContent, ":71A:", "\n", "DEFAULT"));
                    cmd.Parameters.AddWithValue("@Block1", SwiftMessageExtractor.ExtractBlock(messageContent, "{1:", "}"));
                    cmd.Parameters.AddWithValue("@Block2", SwiftMessageExtractor.ExtractBlock(messageContent, "{2:", "}"));
                    cmd.Parameters.AddWithValue("@Block3", SwiftMessageExtractor.ExtractBlock(messageContent, "{3:", "}}"));
                    cmd.Parameters.AddWithValue("@Block4", SwiftMessageExtractor.ExtractBlock(messageContent, "{4:", "-}"));

                    int messageId = (int)cmd.ExecuteScalar();
                    transaction.Commit();
                    return messageId;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return 0;
                }
            }
        }
    }
}
