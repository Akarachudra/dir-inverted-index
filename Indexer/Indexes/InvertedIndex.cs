﻿using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    public class InvertedIndex : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, HashSet<StoredResult>> suffixArray;
        // TODO: Implement memLog on red-black tree
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;
        private readonly object syncObj;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, HashSet<StoredResult>>();
            this.matchComparer = StringComparer.Ordinal;
            this.prefixComparer = new PrefixStringComparer();
            this.syncObj = new object();
        }

        public void Add(string line, int rowNumber, string document)
        {
            var tokens = this.tokenizer.GetTokens(line);
            foreach (var token in tokens)
            {
                this.AddToken(token, rowNumber, document);
            }
        }

        public IList<StoredResult> Find(string query)
        {
            var emptyResult = new List<StoredResult>();
            var tokens = this.tokenizer.GetTokens(query);
            var count = tokens.Count;
            if (count == 0)
            {
                return emptyResult;
            }

            if (count == 1)
            {
                if (this.suffixArray.TryGetValue(tokens[0].Term, out HashSet<StoredResult>[] sets, this.prefixComparer))
                {
                    return this.ConcatHashSetsToList(sets);
                }
            }
            else
            {
                var lastIndex = count - 1;
                var sets = new HashSet<StoredResult>[count][];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    var comparer = this.matchComparer;
                    if (i == lastIndex)
                    {
                        comparer = this.prefixComparer;
                    }

                    if (!this.suffixArray.TryGetValue(term, out sets[i], comparer))
                    {
                        return emptyResult;
                    }
                }

                return this.GetPhraseMatches(tokens, sets);
            }

            return emptyResult;
        }

        private IList<StoredResult> ConcatHashSetsToList(HashSet<StoredResult>[] sets)
        {
            var result = new List<StoredResult>();
            lock (this.syncObj)
            {
                foreach (var set in sets)
                {
                    foreach (var storedResult in set)
                    {
                        result.Add(storedResult);
                    }
                }
            }

            return result;
        }

        private IList<StoredResult> GetPhraseMatches(IList<Token> tokens, HashSet<StoredResult>[][] sets)
        {
            var resultList = new List<StoredResult>();
            var suffixesCount = tokens.Count;
            lock (this.syncObj)
            {
                foreach (var storedResult in sets[0][0])
                {
                    var currentOffset = tokens[0].DistanceToNext;
                    for (var j = 1; j < suffixesCount; j++)
                    {
                        var expectedNextResult = new StoredResult
                        {
                            Document = storedResult.Document,
                            RowNumber = storedResult.RowNumber,
                            ColNumber = storedResult.ColNumber + currentOffset
                        };

                        var containsPhrase = sets[j].Aggregate(false, (current, set) => current | set.Contains(expectedNextResult));
                        if (!containsPhrase)
                        {
                            break;
                        }

                        currentOffset += tokens[j].DistanceToNext;
                        if (j == suffixesCount - 1)
                        {
                            resultList.Add(storedResult);
                        }
                    }
                }
            }

            return resultList;
        }

        private void AddToken(Token token, int rowNumber, string document)
        {
            var startColNumber = token.Position;
            var term = token.Term;
            var length = term.Length;
            for (var i = 0; i < term.Length; i++)
            {
                var storedResult = new StoredResult
                {
                    ColNumber = startColNumber + i,
                    Document = document,
                    RowNumber = rowNumber
                };

                var suffix = term.Substring(i, length - i);
                lock (this.syncObj)
                {
                    if (!this.suffixArray.TryGetValue(suffix, out HashSet<StoredResult> set, this.matchComparer))
                    {
                        this.suffixArray.TryAdd(suffix, new HashSet<StoredResult> { storedResult }, this.matchComparer);
                    }
                    else
                    {
                        set.Add(storedResult);
                    }
                }
            }
        }
    }
}