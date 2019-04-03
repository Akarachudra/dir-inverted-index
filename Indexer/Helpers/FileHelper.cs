using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Indexer.Helpers
{
    public static class FileHelper
    {
        public static IList<FileInfo> GetAllFiles(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        }

        public static bool IsTextFile(string path)
        {
            const int checkLinesCount = 2;
            try
            {
                var processedCount = 0;
                foreach (var line in File.ReadLines(path))
                {
                    if (HasBinaryContentInLine(line))
                    {
                        return false;
                    }

                    processedCount++;
                    if (processedCount >= checkLinesCount)
                    {
                        break;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool HasBinaryContentInLine(string line)
        {
            return line.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
        }
    }
}