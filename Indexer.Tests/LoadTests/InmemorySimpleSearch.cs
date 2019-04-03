using System.Collections.Generic;
using Indexer.Helpers;
using Indexer.Indexes;

namespace Indexer.Tests.LoadTests
{
    public static class InmemorySimpleSearch
    {
        public static IList<DocumentPosition> Find(string[] lines, string phrase)
        {
            var result = new List<DocumentPosition>();
            if (string.IsNullOrEmpty(phrase))
            {
                return result;
            }

            phrase = phrase.Trim();
            var rowNumber = 1;

            foreach (var line in lines)
            {
                foreach (var index in StringHelper.AllIndexesOf(line, phrase))
                {
                    result.Add(
                        new DocumentPosition
                        {
                            ColNumber = index + 1,
                            RowNumber = rowNumber
                        });
                }

                rowNumber++;
            }

            return result;
        }
    }
}