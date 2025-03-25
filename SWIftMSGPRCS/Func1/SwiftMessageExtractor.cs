using System;
using System.Text.RegularExpressions;

namespace SWIftMSGPRCS.Func1
{
    internal static class SwiftMessageExtractor
    {
        public static string ExtractField(string message, string startTag, string endTag, string defaultValue = "DEFAULT")
        {
            int startIndex = message.IndexOf(startTag, StringComparison.Ordinal);
            if (startIndex == -1) return defaultValue;

            startIndex += startTag.Length;
            int endIndex = message.IndexOf(endTag, startIndex);

            return endIndex == -1 ? message.Substring(startIndex).Trim() : message.Substring(startIndex, endIndex - startIndex).Trim();
        }

        public static string ExtractMultiLineField(string message, string fieldTag)
        {
            Match match = Regex.Match(message, fieldTag + "(.*?)(?=\r\n:|\n:|$)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : "DEFAULT";
        }

        public static string ExtractBlock(string message, string startTag, string endTag)
        {
            int startIndex = message.IndexOf(startTag, StringComparison.Ordinal);
            if (startIndex == -1) return "DEFAULT";

            int endIndex = message.IndexOf(endTag, startIndex + startTag.Length, StringComparison.Ordinal);
            return endIndex == -1 ? message.Substring(startIndex).Trim() : message.Substring(startIndex, endIndex - startIndex).Trim();
        }

        public static string ExtractOwnBIC(string message)
        {
            string block1 = ExtractBlock(message, "{1:", "}");
            return block1.Length >= 15 ? block1.Substring(6, 12) : "DEFAULT";
        }

        public static string ExtractMessageType(string message)
        {
            string block2 = ExtractBlock(message, "{2:", "}");
            return block2.Length >= 7 ? block2.Substring(4, 3) : "DEFAULT";
        }

        public static string ExtractSubFormat(string message)
        {
            return ExtractBlock(message, "{2:", "}").StartsWith("{2:O") ? "OUTPUT" : "INPUT";
        }

        public static string ExtractCorBIC(string message)
        {
            string block2 = ExtractBlock(message, "{2:", "}");
            if (string.IsNullOrEmpty(block2)) return "DEFAULT";

            string pattern = block2.StartsWith("{2:I")
                ? @"\{2:I\d{3}([A-Z0-9]{12})"
                : @"\{2:O\d{3}0{10}([A-Z0-9]{12})";

            Match match = Regex.Match(block2, pattern);
            return match.Success ? match.Groups[1].Value : "DEFAULT";
        }
    }
}
