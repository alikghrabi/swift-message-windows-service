using System;
using System.IO;
using SWIftMSGPRCS.Func1;

namespace SWIftMSGPRCS.Func1
{
    internal class SwiftMsgPrcs
    {
        private readonly string _watchPath;
        private readonly string _successPath;
        private readonly string _errorPath;
        private readonly DatabaseService _dbService;

        public SwiftMsgPrcs(string watchPath, string successPath, string errorPath, string connectionString)
        {
            _watchPath = watchPath;
            _successPath = successPath;
            _errorPath = errorPath;
            _dbService = new DatabaseService(connectionString);
        }

        public void ProcessSwiftMessages()
        {
            foreach (var file in Directory.GetFiles(_watchPath, "*.txt"))
            {
                ProcessFile(file);
            }
        }

        private void ProcessFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            int fileId = _dbService.InsertMessageFile(content);

            foreach (string message in content.Split(new[] { "$" }, StringSplitOptions.RemoveEmptyEntries))
            {
                _dbService.InsertMessage(fileId, message);
            }

            File.Move(filePath, Path.Combine(_successPath, Path.GetFileName(filePath)));
        }
    }
}
