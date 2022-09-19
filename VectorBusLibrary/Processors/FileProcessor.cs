using System;
using System.IO;
using System.Threading.Tasks;

namespace VectorBusLibrary.Processors
{
    public static class FileProcessor
    {

        public static async Task SaveTextToFileAsync(string text, string filePath = "defaultLog.csv")
        {
            string textToSave = $"{text};{Environment.NewLine}";

            await File.AppendAllTextAsync(filePath, textToSave);
        }
    }
}
