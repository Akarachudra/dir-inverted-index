﻿using System.Collections.Generic;
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

        public IList<SearchResult> Find(string term)
        {
            var result = new List<SearchResult>();
            var directoryInfo = new DirectoryInfo(this.filesPath);
            foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                var rowNumber = 1;

                foreach (var line in File.ReadLines(file.FullName))
                {
                    foreach (Match match in Regex.Matches(line, term))
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