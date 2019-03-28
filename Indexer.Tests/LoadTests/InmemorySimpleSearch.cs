using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                foreach (Match match in Regex.Matches(line, phrase, RegexOptions.IgnoreCase))
                {
                    result.Add(
                        new StoredResult
                        {
                            ColNumber = match.Index + 1,
                            RowNumber = rowNumber
                        });
                }

                rowNumber++;
            }

            return result;
        }
    }
}