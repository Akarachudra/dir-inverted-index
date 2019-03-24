using System.Collections.Generic;
using System.IO;

namespace Indexer.Helpers
{
    public static class FileHelper
    {
        public static IList<FileInfo> GetAllFiles(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        }
    }
}