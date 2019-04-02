using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Indexer.Helpers;
using Indexer.Indexes;

namespace Indexer
{
    public class SimpleDirectorySearcher : ISearcher
    {
        private readonly string filesPath;

        public SimpleDirectorySearcher(string filesPath)
        {
            this.filesPath = filesPath;
        }

        public IList<DocumentPosition> Find(string phrase)
        {
            var result = new List<DocumentPosition>();
            var files = FileHelper.GetAllFiles(this.filesPath);
            foreach (var file in files)
            {
                var rowNumber = 1;

                foreach (var line in File.ReadLines(file.FullName))
                {
                    foreach (Match match in Regex.Matches(line, phrase, RegexOptions.IgnoreCase))
                    {
                        result.Add(
                            new DocumentPosition
                            {
                                Document = file.FullName,
                                ColNumber = match.Index + 1,
                                RowNumber = rowNumber
                            });
                    }

                    rowNumber++;
                }
            }

            return result;
        }
    }
}