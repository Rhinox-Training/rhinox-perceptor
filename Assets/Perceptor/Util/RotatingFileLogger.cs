using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rhinox.Perceptor
{
    public class RotatingFileLogger
    {
        public string FilePath { get; }
        private string BackFilePath { get; }
        public int MaxFileSize { get; }
        
        public const int DEFAULT_MAX_FILE_SIZE = 10 * 1024 * 1024;

        private static Dictionary<string, object> _locks = new Dictionary<string,object>();

        public RotatingFileLogger(string filePath, int maxFileSize = DEFAULT_MAX_FILE_SIZE)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string absPath = Path.GetFullPath(filePath);
            string containingFolder = Directory.GetParent(absPath).FullName;
            FilePath = absPath;
            BackFilePath = Path.GetFullPath(Path.Combine(containingFolder, $"{fileName}-prev.{extension}"));
            MaxFileSize = maxFileSize;
            if (_locks == null)
                _locks = new Dictionary<string, object>();
            if (!_locks.ContainsKey(FilePath))
                _locks.Add(FilePath, new object());
        }

        public void WriteLine(string line)
        {
            lock(_locks[FilePath])
            {
                FileInfo fi = new FileInfo(FilePath);
                int utfLength = Encoding.UTF8.GetByteCount(line);
                if (fi.Exists && fi.Length + utfLength > DEFAULT_MAX_FILE_SIZE)
                    SwapFile();
                TryCreateDirectories(FilePath);
                File.AppendAllText(FilePath, line + "\n");
            }
        }

        private void SwapFile()
        {
            TryCreateDirectories(BackFilePath);
            File.Copy(FilePath, BackFilePath, true);
            using(FileStream fs = File.Open(FilePath ,FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock(fs)
                {
                    fs.SetLength(0);
                }
            }
        }

        private static bool TryCreateDirectories(string path)
        {
            var fi = new FileInfo(path);
            try
            {
                if (fi.Directory != null && !fi.Directory.Exists)
                    Directory.CreateDirectory(fi.Directory.FullName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}