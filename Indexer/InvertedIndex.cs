using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer
{
    public class InvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, IList<StoredResult>> suffixArray;
        private readonly IComparer<string> insertComparer;
        private readonly IComparer<string> readComparer;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, IList<StoredResult>>();
            this.insertComparer = StringComparer.Ordinal;
            this.readComparer = new PrefixStringComparer();
        }

        public void Add(string line, int rowNumber, string pathHash)
        {
            var tokens = this.tokenizer.GetTokens(line);
            foreach (var token in tokens)
            {
                this.AddToken(token, rowNumber, pathHash);
            }
        }

        public IList<StoredResult> Find(string query)
        {
            var tokens = this.tokenizer.GetTokens(query);
            var count = tokens.Count;
            if (count == 1)
            {
                if (this.suffixArray.TryGetValue(tokens[0].Term, out var list, this.readComparer))
                {
                    return list;
                }
            }
            else
            {
                var lists = new IList<StoredResult>[count];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    if (!this.suffixArray.TryGetValue(term, out lists[i], this.readComparer))
                    {
                        return new List<StoredResult>();
                    }
                }

                return GetPhraseMatches(tokens.Select(x => x.Term).ToArray(), lists);
            }

            return new List<StoredResult>();
        }

        private static IList<StoredResult> GetPhraseMatches(string[] terms, IList<StoredResult>[] lists)
        {
            var resultList = new List<StoredResult>();
            var suffixesCount = terms.Length;
            var suffix = terms[0];
            var currentOffset = suffix.Length;
            for (var i = 0; i < lists[0].Count; i++)
            {
                var currentResult = lists[0][i];
                for (var j = 1; j < suffixesCount; j++)
                {
                    var expectedNextResult = new StoredResult
                    {
                        PathHash = currentResult.PathHash,
                        RowNumber = currentResult.RowNumber,
                        ColNumber = currentResult.ColNumber + currentOffset
                    };
                    if (!ListContains(lists[j], expectedNextResult))
                    {
                        break;
                    }

                    currentOffset += terms[j].Length;
                    if (j == suffixesCount - 1)
                    {
                        resultList.Add(currentResult);
                    }
                }
            }

            return resultList;
        }

        private static bool ListContains(IList<StoredResult> list, StoredResult result)
        {
            foreach (var searchResult in list)
            {
                if (result.RowNumber == searchResult.RowNumber && result.ColNumber == searchResult.ColNumber && result.PathHash == searchResult.PathHash)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddToken(Token token, int rowNumber, string pathHash)
        {
            var startColnumber = token.Position;
            var term = token.Term;
            term = term.ToLowerInvariant();
            var length = term.Length;
            for (var i = 0; i < term.Length; i++)
            {
                var storedResult = new StoredResult
                {
                    ColNumber = startColnumber + i,
                    PathHash = pathHash,
                    RowNumber = rowNumber
                };

                var suffix = term.Substring(i, length - i);
                if (!this.suffixArray.TryGetValue(suffix, out var list, this.insertComparer))
                {
                    this.suffixArray.TryAdd(suffix, new List<StoredResult> { storedResult }, this.insertComparer);
                }
                else
                {
                    list.Add(storedResult);
                }
            }
        }
    }
}