using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    // TODO: impl change, rename, delete document
    public class InvertedIndex : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, HashSet<DocumentPosition>> suffixArray;
        // TODO: Implement memLog on red-black tree
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;
        private readonly object syncObj;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, HashSet<DocumentPosition>>();
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

        public IList<DocumentPosition> Find(string query)
        {
            var emptyResult = new List<DocumentPosition>();
            var tokens = this.tokenizer.GetTokens(query);
            var count = tokens.Count;
            if (count == 0)
            {
                return emptyResult;
            }

            if (count == 1)
            {
                if (this.suffixArray.TryGetRangeValue(tokens[0].Term, out HashSet<DocumentPosition>[] sets, this.prefixComparer))
                {
                    return this.ConcatHashSetsToList(sets);
                }
            }
            else
            {
                var lastIndex = count - 1;
                var sets = new HashSet<DocumentPosition>[count][];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    var comparer = this.matchComparer;
                    if (i == lastIndex)
                    {
                        comparer = this.prefixComparer;
                    }

                    if (!this.suffixArray.TryGetRangeValue(term, out sets[i], comparer))
                    {
                        return emptyResult;
                    }
                }

                return this.GetPhraseMatches(tokens, sets);
            }

            return emptyResult;
        }

        private IList<DocumentPosition> ConcatHashSetsToList(HashSet<DocumentPosition>[] sets)
        {
            var result = new List<DocumentPosition>();
            lock (this.syncObj)
            {
                foreach (var set in sets)
                {
                    foreach (var documentPosition in set)
                    {
                        result.Add(documentPosition);
                    }
                }
            }

            return result;
        }

        private IList<DocumentPosition> GetPhraseMatches(IList<Token> tokens, HashSet<DocumentPosition>[][] sets)
        {
            var resultList = new List<DocumentPosition>();
            var suffixesCount = tokens.Count;
            lock (this.syncObj)
            {
                foreach (var documentPosition in sets[0][0])
                {
                    var currentOffset = tokens[0].DistanceToNext;
                    for (var j = 1; j < suffixesCount; j++)
                    {
                        var expectedNextResult = new DocumentPosition
                        {
                            Document = documentPosition.Document,
                            RowNumber = documentPosition.RowNumber,
                            ColNumber = documentPosition.ColNumber + currentOffset
                        };

                        var containsPhrase = sets[j].Aggregate(false, (current, set) => current | set.Contains(expectedNextResult));
                        if (!containsPhrase)
                        {
                            break;
                        }

                        currentOffset += tokens[j].DistanceToNext;
                        if (j == suffixesCount - 1)
                        {
                            resultList.Add(documentPosition);
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
                var documentPosition = new DocumentPosition
                {
                    ColNumber = startColNumber + i,
                    Document = document,
                    RowNumber = rowNumber
                };

                var suffix = term.Substring(i, length - i);
                lock (this.syncObj)
                {
                    if (!this.suffixArray.TryGetValue(suffix, out HashSet<DocumentPosition> set, this.matchComparer))
                    {
                        this.suffixArray.TryAdd(suffix, new HashSet<DocumentPosition> { documentPosition }, this.matchComparer);
                    }
                    else
                    {
                        set.Add(documentPosition);
                    }
                }
            }
        }
    }
}