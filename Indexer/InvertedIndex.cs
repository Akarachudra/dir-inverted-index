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

            var suffix = suffixes[0];
            var currentOffset = suffix.Length + 1;
            for (var j = 0; j < lists[0].Count; j++)
            {
                var currentResult = lists[0][j];
                for (var m = 1; m < suffixes.Length; m++)
                {
                    if (!HasMatch(
                        new SearchResult
                            { FilePath = currentResult.FilePath, RowNumber = currentResult.RowNumber, ColNumber = currentResult.ColNumber + currentOffset },
                        lists[m]))
                    {
                        break;
                    }

                    currentOffset += suffixes[m].Length + 1;
                    if (m == suffixes.Length - 1)
                    {
                        resultList.Add(currentResult);
                    }
                }
            }

            return resultList;
        }

        private static bool HasMatch(SearchResult result, IList<SearchResult> list)
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