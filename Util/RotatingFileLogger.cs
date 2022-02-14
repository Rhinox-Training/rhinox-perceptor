using System;
using System.IO;
using System.Text;

namespace Perceptor.Util
{
    public class RotatingFileLogger
    {
        public string FilePath { get; }
        private string BackFilePath { get; }
        public int MaxFileSize { get; }
    
    
        public const int DEFAULT_MAX_FILE_SIZE = 10 * 1024 * 1024;
    
        public RotatingFileLogger(string filePath, int maxFileSize = DEFAULT_MAX_FILE_SIZE)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string absPath = Path.GetFullPath(filePath);
            string containingFolder = Directory.GetParent(absPath).FullName;
            FilePath = absPath;
            BackFilePath = Path.GetFullPath(Path.Combine(containingFolder, $"{fileName}-prev.{extension}"));
            MaxFileSize = maxFileSize;
        }

        public void WriteLine(string line)
        {
            FileInfo fi = new FileInfo(FilePath);
            int utfLength = Encoding.UTF8.GetByteCount(line);
            if (fi.Exists && fi.Length + utfLength > DEFAULT_MAX_FILE_SIZE)
                SwapFile();
            File.AppendAllText(FilePath, line + "\n");
        }

        private void SwapFile()
        {
            File.Copy(FilePath, BackFilePath, true);
            using(FileStream fs = File.Open(FilePath ,FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock(fs)
                {
                    fs.SetLength(0);
                }
            }
        }
    }
}