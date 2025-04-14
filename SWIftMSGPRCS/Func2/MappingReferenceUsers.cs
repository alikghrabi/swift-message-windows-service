using System;
using System.IO;
using System.Data.SqlClient;

namespace SWIftMSGPRCS.Func2
{
    internal class MappingReferenceUsers
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly FileHandler _fileHandler;

        public MappingReferenceUsers(string connectionString)
        {
            _dbHelper = new DatabaseHelper(connectionString);
            _fileHandler = new FileHandler();
        }

        public void ProcessMessages()
        {
            Console.WriteLine("Checking for new messages to process...");

            using (var connection = _dbHelper.OpenConnection())
            {
                var messages = _dbHelper.GetMessagesToProcess(connection);

                foreach (var msg in messages)
                {
                    Console.WriteLine($"Processing Message ID: {msg.IdKey}, UserReference: {msg.UserReference}");

                    if (string.IsNullOrEmpty(msg.UserReference))
                    {
                        Console.WriteLine($"Skipping Message ID {msg.IdKey} due to empty UserReference.");
                        continue;
                    }

                    string mapValue = _dbHelper.GetMappingForUserReference(msg.UserReference, connection);

                    if (!string.IsNullOrEmpty(mapValue))
                    {
                        _fileHandler.SaveMessageToFile(mapValue, msg.MessageContent);
                        _dbHelper.UpdateMessageStatus(msg.IdKey, "MesgProcessed", connection);
                    }
                    else
                    {
                        Console.WriteLine($"No mapping found for UserReference {msg.UserReference}. Skipping.");
                    }
                }
            }
        }
    }
}
