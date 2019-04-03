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
        private readonly SuffixArray<string, List<DocumentPosition>> suffixArray;
        private readonly Dictionary<string, string[]> indexedFiles;
        // TODO: Implement memLog on red-black tree
        private readonly IComparer<string> matchComparer;
        private readonly IComparer<string> prefixComparer;
        private readonly object syncObj;

        public InvertedIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.suffixArray = new SuffixArray<string, List<DocumentPosition>>();
            this.matchComparer = StringComparer.Ordinal;
            this.prefixComparer = new PrefixStringComparer();
            this.syncObj = new object();
            this.indexedFiles = new Dictionary<string, string[]>();
        }

        public void Add(string[] lines, string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                throw new ArgumentException("Document could not be null or whitespace");
            }

            lock (this.syncObj)
            {
                if (!this.indexedFiles.ContainsKey(document))
                {
                    var loweredLines = new string[lines.Length];
                    for (var i = 0; i < lines.Length; i++)
                    {
                        loweredLines[i] = lines[i].ToLowerInvariant();
                    }

                    this.indexedFiles[document] = loweredLines;
                }
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var tokens = this.tokenizer.GetTokens(lines[i]);

                foreach (var token in tokens)
                {
                    this.AddToken(token, i + 1, document);
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
                if (this.suffixArray.TryGetRangeValue(tokens[0].Term, out var lists, this.prefixComparer))
                {
                    return this.ConcatLists(lists);
                }
            }
            else
            {
                var term = tokens[0].Term;
                if (!this.suffixArray.TryGetValue(term, out var firstList, this.matchComparer))
                {
                    return emptyResult;
                }

                return this.GetPhraseMatches(query.ToLowerInvariant(), firstList);
            }

            return emptyResult;
        }

        private IList<DocumentPosition> ConcatLists(IEnumerable<List<DocumentPosition>> lists)
        {
            var result = new List<DocumentPosition>();
            lock (this.syncObj)
            {
                result.AddRange(lists.SelectMany(list => list));
            }

            return result;
        }

        private IList<DocumentPosition> GetPhraseMatches(string phrase, IEnumerable<DocumentPosition> firstList)
        {
            var resultList = new List<DocumentPosition>();
            lock (this.syncObj)
            {
                foreach (var documentPosition in firstList)
                {
                    var rowNumber = documentPosition.RowNumber - 1 < 0 ? 0 : documentPosition.RowNumber - 1;
                    var line = this.indexedFiles[documentPosition.Document][rowNumber];
                    if (line.Length < documentPosition.ColNumber - 1 + phrase.Length)
                    {
                        continue;
                    }

                    if (line.Substring(documentPosition.ColNumber - 1, phrase.Length) == phrase)
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
                lock (this.syncObj)
                {
                    if (!this.suffixArray.TryGetValue(suffix, out var list, this.matchComparer))
                    {
                        this.suffixArray.TryAdd(suffix, new List<DocumentPosition> { documentPosition }, this.matchComparer);
                    }
                    else
                    {
                        list.Add(documentPosition);
                    }
                }
            }
        }
    }
}