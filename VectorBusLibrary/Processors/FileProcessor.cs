using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBusLibrary.Processors
{
    public static class FileProcessor
    {

        public static async Task SaveTextToFileAsync(string text,string filePath = "defaultLog.csv")
        {
            string textToSave = $"{text};{Environment.NewLine}";

            await File.AppendAllTextAsync(filePath, textToSave);
        }
    }
}
