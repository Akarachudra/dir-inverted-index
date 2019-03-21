using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Indexer
{
    public class SimpleSearcher : ISearchService
    {
        private readonly string filesPath;

        public SimpleSearcher(string filesPath)
        {
            this.filesPath = filesPath;
        }

        public IList<SearchResult> FindString(string searchQuery)
        {
            var result = new List<SearchResult>();
            var di = new DirectoryInfo(this.filesPath);
            foreach (var file in di.GetFiles())
            {
                var rowNumber = 1;

                foreach (var line in File.ReadLines(file.FullName))
                {
                    foreach (Match match in Regex.Matches(line, searchQuery))
                    {
                        result.Add(
                            new SearchResult
                            {
                                FilePath = file.FullName,
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