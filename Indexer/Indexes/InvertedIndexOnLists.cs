using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Collections;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    public class InvertedIndexOnLists : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly SuffixArray<string, LinkedList<DocumentPosition>> suffixArray;
        // TODO: Use red-black -tree or something else
        private readonly SuffixArray<string, LinkedList<DocumentPosition>> memLog;
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;
        private readonly object addLock;

        public InvertedIndexOnLists(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, LinkedList<DocumentPosition>>();
            this.matchComparer = StringComparer.Ordinal;
            this.prefixComparer = new PrefixStringComparer();
            this.addLock = new object();
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
                if (this.suffixArray.TryGetRangeValue(tokens[0].Term, out LinkedList<DocumentPosition>[] lists, this.prefixComparer))
                {
                    return ConcatsLinkedLists(lists);
                }
            }
            else
            {
                var lastIndex = count - 1;
                var lists = new LinkedList<DocumentPosition>[count][];
                for (var i = 0; i < count; i++)
                {
                    var term = tokens[i].Term;
                    var comparer = this.matchComparer;
                    if (i == lastIndex)
                    {
                        comparer = this.prefixComparer;
                    }

                    if (!this.suffixArray.TryGetRangeValue(term, out lists[i], comparer))
                    {
                        return emptyResult;
                    }
                }

                return GetPhraseMatches(tokens, lists);
            }

            return emptyResult;
        }

        private static IList<DocumentPosition> ConcatsLinkedLists(LinkedList<DocumentPosition>[] lists)
        {
            IEnumerable<DocumentPosition> concated = lists[0];
            for (var i = 1; i < lists.Length; i++)
            {
                concated = concated.Concat(lists[i]);
            }

            return concated.ToList();
        }

        private static IList<DocumentPosition> GetPhraseMatches(IList<Token> tokens, LinkedList<DocumentPosition>[][] lists)
        {
            var resultList = new List<DocumentPosition>();
            var suffixesCount = tokens.Count;
            foreach (var documentPosition in lists[0][0])
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

                    var containsPhrase = lists[j].Aggregate(false, (current, set) => current | set.Contains(expectedNextResult));
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
                lock (this.addLock)
                {
                    if (!this.suffixArray.TryGetValue(suffix, out LinkedList<DocumentPosition> list, this.matchComparer))
                    {
                        this.suffixArray.TryAdd(suffix, new LinkedList<DocumentPosition>(new[] { documentPosition }), this.matchComparer);
                    }
                    else
                    {
                        this.AppendToList(list, documentPosition);
                    }
                }
            }
        }

        private void AppendToList(LinkedList<DocumentPosition> list, DocumentPosition result)
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