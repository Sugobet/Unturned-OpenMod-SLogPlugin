using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOpenModPlugin
{
    public class FileCTL
    {
        static string _dirPath = Path.Combine(Environment.CurrentDirectory, "SLog");
        static string _fileName = _dirPath + "/SLog.txt";
        
        public static async Task AppendAllTextAsync(string text)
        {
            try
            {
                using (FileStream fileStream = new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    await streamWriter.WriteLineAsync(text);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
