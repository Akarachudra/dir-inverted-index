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
        private readonly SuffixArray<string, HashSet<StoredResult>> suffixArray;
        private readonly IComparer<string> insertComparer;
        private readonly IComparer<string> readComparer;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, HashSet<StoredResult>>();
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
                if (this.suffixArray.TryGetValue(tokens[0].Term, out HashSet<StoredResult>[] sets, this.readComparer))
                {
                    return ConcatHashSets(sets);
                }
            }
            else
            {
                var sets = new HashSet<StoredResult>[count][];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    if (!this.suffixArray.TryGetValue(term, out sets[i], this.readComparer))
                    {
                        return new List<StoredResult>();
                    }
                }

                return GetPhraseMatches(tokens.Select(x => x.Term).ToArray(), sets);
            }

            return new List<StoredResult>();
        }

        private static IList<StoredResult> ConcatHashSets(HashSet<StoredResult>[] sets)
        {
            IEnumerable<StoredResult> concated = sets[0];
            for (var i = 1; i < sets.Length; i++)
            {
                concated = concated.Concat(sets[i]);
            }

            return concated.ToList();
        }

        private static IList<StoredResult> GetPhraseMatches(string[] terms, HashSet<StoredResult>[][] sets)
        {
            var resultList = new List<StoredResult>();
            var suffixesCount = terms.Length;
            var suffix = terms[0];
            var currentOffset = suffix.Length;
            for (var x = 0; x < sets[0].Length; x++)
            {
                for (var i = 0; i < sets[0][x].Count; i++)
                {
                    foreach (var storedResult in sets[0][x])
                    {
                        for (var j = 1; j < suffixesCount; j++)
                        {
                            var expectedNextResult = new StoredResult
                            {
                                PathHash = storedResult.PathHash,
                                RowNumber = storedResult.RowNumber,
                                ColNumber = storedResult.ColNumber + currentOffset
                            };

                            var contains = false;
                            foreach (var set in sets[j])
                            {
                                contains = contains | set.Contains(expectedNextResult);
                            }

                            if (!contains)
                            {
                                break;
                            }

                            currentOffset += terms[j].Length;
                            if (j == suffixesCount - 1)
                            {
                                resultList.Add(storedResult);
                            }
                        }
                    }
                }
            }

            return resultList;
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
                if (!this.suffixArray.TryGetValue(suffix, out HashSet<StoredResult> set, this.insertComparer))
                {
                    this.suffixArray.TryAdd(suffix, new HashSet<StoredResult> { storedResult }, this.insertComparer);
                }
                else
                {
                    set.Add(storedResult);
                }
            }
        }
    }
}