using System;
using System.Collections.Generic;
using Indexer.Collections;

namespace Indexer
{
    public class InvertedIndex
    {
        private readonly SuffixArray<string, IList<SearchResult>> suffixArray;
        private readonly IComparer<string> insertComparer;
        private readonly IComparer<string> readComparer;

        public InvertedIndex()
        {
            this.suffixArray = new SuffixArray<string, IList<SearchResult>>();
            this.insertComparer = StringComparer.Ordinal;
            this.readComparer = new PrefixStringComparer();
        }

        public void Add(string word, SearchResult curResult)
        {
            word = word.ToLower();
            var length = word.Length;
            for (var i = 0; i < word.Length; i++)
            {
                curResult = new SearchResult
                {
                    ColNumber = curResult.ColNumber + i,
                    FilePath = curResult.FilePath,
                    RowNumber = curResult.RowNumber
                };

                var suffix = word.Substring(i, length - i);
                if (!this.suffixArray.TryGetValue(suffix, out var list, this.insertComparer))
                {
                    this.suffixArray.TryAdd(suffix, new List<SearchResult> { curResult }, this.insertComparer);
                }
                else
                {
                    list.Add(curResult);
                }
            }
        }

        public IList<SearchResult> Find(string query)
        {
            query = query.ToLower();
            if (this.suffixArray.TryGetValue(query, out var list, this.readComparer))
            {
                return list;
            }

            return new List<SearchResult>();
        }
    }
}