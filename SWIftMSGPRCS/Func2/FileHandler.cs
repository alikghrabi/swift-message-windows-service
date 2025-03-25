using System;
using System.IO;

namespace SWIftMSGPRCS.Func2
{
    internal class FileHandler
    {
        public void SaveMessageToFile(string directoryPath, string messageContent)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Created directory: {directoryPath}");
                }

                string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                string filePath = Path.Combine(directoryPath, fileName);

                File.WriteAllText(filePath, messageContent);
                Console.WriteLine($"File created: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing file: {ex.Message}");
            }
        }
    }
}
