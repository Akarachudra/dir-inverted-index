using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    public class InvertedIndexWithLists : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, LinkedList<StoredResult>> suffixArray;
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;

        public InvertedIndexWithLists(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, LinkedList<StoredResult>>();
            this.matchComparer = StringComparer.Ordinal;
            this.prefixComparer = new PrefixStringComparer();
        }

        public void Add(string line, int rowNumber, string document)
        {
            var tokens = this.tokenizer.GetTokens(line);
            foreach (var token in tokens)
            {
                if (token.Term.Length < 100)
                {
                    this.AddToken(token, rowNumber, document);
                }
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
                if (this.suffixArray.TryGetValue(tokens[0].Term, out LinkedList<StoredResult>[] lists, this.prefixComparer))
                {
                    return ConcatsLinkedLists(lists);
                }
            }
            else
            {
                var lastIndex = count - 1;
                var lists = new LinkedList<StoredResult>[count][];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    var comparer = this.matchComparer;
                    if (i == lastIndex)
                    {
                        comparer = this.prefixComparer;
                    }

                    if (!this.suffixArray.TryGetValue(term, out lists[i], comparer))
                    {
                        return emptyResult;
                    }
                }

                return GetPhraseMatches(tokens, lists);
            }

            return emptyResult;
        }

        private static IList<StoredResult> ConcatsLinkedLists(LinkedList<StoredResult>[] sets)
        {
            IEnumerable<StoredResult> concated = sets[0];
            for (var i = 1; i < sets.Length; i++)
            {
                concated = concated.Concat(sets[i]);
            }

            return concated.ToList();
        }

        private static IList<StoredResult> GetPhraseMatches(IList<Token> tokens, LinkedList<StoredResult>[][] sets)
        {
            var resultList = new List<StoredResult>();
            var suffixesCount = tokens.Count;
            var currentOffset = tokens[0].DistanceToNext;
            for (var i = 0; i < sets[0].Length; i++)
            {
                foreach (var storedResult in sets[0][i])
                {
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
                if (!this.suffixArray.TryGetValue(suffix, out LinkedList<StoredResult> list, this.matchComparer))
                {
                    this.suffixArray.TryAdd(suffix, new LinkedList<StoredResult>(new[] { storedResult }), this.matchComparer);
                }
                else
                {
                    this.AppendToList(list, storedResult);
                }
            }
        }

        private void AppendToList(LinkedList<StoredResult> list, StoredResult result)
        {
            if (list.Count == 0)
            {
                list.AddLast(result);
            }
            else
            {
                var node = list.First;
                while (node.Next != null && (string.Compare(node.Value.Document, result.Document, StringComparison.Ordinal) > 0 ||
                                             node.Value.RowNumber > result.RowNumber || node.Value.ColNumber > result.ColNumber))
                {
                    node = node.Next;
                }

                list.AddAfter(node, result);
            }
        }
    }
}