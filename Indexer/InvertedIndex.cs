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
            var startColnumber = curResult.ColNumber;
            word = word.ToLowerInvariant();
            var length = word.Length;
            for (var i = 0; i < word.Length; i++)
            {
                curResult = new SearchResult
                {
                    ColNumber = startColnumber + i,
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
            query = query.ToLowerInvariant();
            var suffixes = query.Split(' ');
            if (suffixes.Length == 1)
            {
                if (this.suffixArray.TryGetValue(query, out var list, this.readComparer))
                {
                    return list;
                }
            }
            else
            {
                var lists = new IList<SearchResult>[suffixes.Length];
                for (var i = 0; i < suffixes.Length; i++)
                {
                    var suffix = suffixes[i];
                    if (!this.suffixArray.TryGetValue(suffix, out lists[i], this.readComparer))
                    {
                        return new List<SearchResult>();
                    }
                }

                return GetPhraseMatches(suffixes, lists);
            }

            return new List<SearchResult>();
        }

        private static IList<SearchResult> GetPhraseMatches(string[] suffixes, IList<SearchResult>[] lists)
        {
            var resultList = new List<SearchResult>();
            var suffixesCount = suffixes.Length;
            var suffix = suffixes[0];
            var currentOffset = suffix.Length + 1;
            for (var i = 0; i < lists[0].Count; i++)
            {
                var currentResult = lists[0][i];
                for (var j = 1; j < suffixesCount; j++)
                {
                    var expectedNextResult = new SearchResult
                    {
                        FilePath = currentResult.FilePath,
                        RowNumber = currentResult.RowNumber,
                        ColNumber = currentResult.ColNumber + currentOffset
                    };
                    if (!ListContains(lists[j], expectedNextResult))
                    {
                        break;
                    }

                    currentOffset += suffixes[j].Length + 1;
                    if (j == suffixesCount - 1)
                    {
                        resultList.Add(currentResult);
                    }
                }
            }

            return resultList;
        }

        private static bool ListContains(IList<SearchResult> list, SearchResult result)
        {
            foreach (var searchResult in list)
            {
                if (result.RowNumber == searchResult.RowNumber && result.ColNumber == searchResult.ColNumber && result.FilePath == searchResult.FilePath)
                {
                    return true;
                }
            }

            return false;
        }
    }
}