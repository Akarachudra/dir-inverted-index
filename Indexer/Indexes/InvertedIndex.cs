using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    public class InvertedIndex : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, HashSet<StoredResult>> immutableSuffixArray;
        private readonly SortedDictionary<string, HashSet<StoredResult>> memLogDictionary;
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;
        private readonly object addLock;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.immutableSuffixArray = new SuffixArray<string, HashSet<StoredResult>>();
            this.memLogDictionary = new SortedDictionary<string, HashSet<StoredResult>>();
            this.matchComparer = StringComparer.Ordinal;
            this.prefixComparer = new PrefixStringComparer();
            this.addLock = new object();
            var list = new List<string>();
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
                if (this.immutableSuffixArray.TryGetValue(tokens[0].Term, out HashSet<StoredResult>[] sets, this.prefixComparer))
                {
                    return ConcatHashSetsToList(sets);
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

                    if (!this.immutableSuffixArray.TryGetValue(term, out sets[i], comparer))
                    {
                        return emptyResult;
                    }
                }

                return GetPhraseMatches(tokens, sets);
            }

            return emptyResult;
        }

        private static IList<StoredResult> ConcatHashSetsToList(HashSet<StoredResult>[] sets)
        {
            IEnumerable<StoredResult> concated = sets[0];
            for (var i = 1; i < sets.Length; i++)
            {
                concated = concated.Concat(sets[i]);
            }

            return concated.ToList();
        }

        private static IList<StoredResult> GetPhraseMatches(IList<Token> tokens, HashSet<StoredResult>[][] sets)
        {
            var resultList = new List<StoredResult>();
            var suffixesCount = tokens.Count;
            for (var i = 0; i < sets[0].Length; i++)
            {
                foreach (var storedResult in sets[0][i])
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
                lock (this.addLock)
                {
                    if (!this.memLogDictionary.TryGetValue(suffix, out var hashSet))
                    {
                        this.immutableSuffixArray.TryAdd(suffix, new HashSet<StoredResult> { storedResult }, this.matchComparer);
                    }
                    else
                    {
                        hashSet.Add(storedResult);
                    }

                    if (!this.immutableSuffixArray.TryGetValue(suffix, out HashSet<StoredResult> set, this.matchComparer))
                    {
                        this.immutableSuffixArray.TryAdd(suffix, new HashSet<StoredResult> { storedResult }, this.matchComparer);
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