using System;
using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer.Tests.LoadTests
{
    public static class InmemorySimpleSearch
    {
        public static IList<StoredResult> Find(string[] lines, string phrase)
        {
            var result = new List<StoredResult>();
            var rowNumber = 1;

            foreach (var line in lines)
            {
                foreach (var index in AllIndexesOf(line, phrase))
                {
                    result.Add(
                        new StoredResult
                        {
                            ColNumber = index + 1,
                            RowNumber = rowNumber
                        });
                }

                rowNumber++;
            }

            return result;
        }

        private static IEnumerable<int> AllIndexesOf(string str, string searchstring)
        {
            var minIndex = str.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}