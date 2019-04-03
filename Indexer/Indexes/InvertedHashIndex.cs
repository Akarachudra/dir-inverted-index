using System;
using System.Collections.Generic;
using System.Linq;
using Indexer.Helpers;
using Indexer.Tokens;

namespace Indexer.Indexes
{
    public class InvertedHashIndex : IInvertedIndex
    {
        private readonly ITokenizer tokenizer;
        private readonly Dictionary<int, HashSet<DocumentPosition>> dictionary;
        private readonly object syncObj;

        public InvertedHashIndex(ITokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.dictionary = new Dictionary<int, HashSet<DocumentPosition>>();
            this.syncObj = new object();
        }

        public void Add(string[] lines, string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                throw new ArgumentException("Document could not be null or whitespace");
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var tokens = this.tokenizer.GetTokens(line);
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
                lock (this.syncObj)
                {
                    if (this.dictionary.TryGetValue(StringHelper.GetHashCode(tokens[0].Term), out var dict))
                    {
                        return dict.ToList();
                    }
                }
            }
            else
            {
                var dictionaries = new HashSet<DocumentPosition>[count];
                for (var i = 0; i < count; i++)
                {
                    lock (this.syncObj)
                    {
                        var term = tokens[i].Term;
                        if (!this.dictionary.TryGetValue(StringHelper.GetHashCode(term), out dictionaries[i]))
                        {
                            return new List<DocumentPosition>();
                        }
                    }
                }

                return this.GetPhraseMatches(tokens, dictionaries);
            }

            return new List<DocumentPosition>();
        }

        private IList<DocumentPosition> GetPhraseMatches(IList<Token> tokens, HashSet<DocumentPosition>[] dictionaries)
        {
            var resultList = new List<DocumentPosition>();
            var suffixesCount = tokens.Count;
            lock (this.syncObj)
            {
                foreach (var currentPosition in dictionaries[0])
                {
                    var currentOffset = tokens[0].DistanceToNext;
                    for (var j = 1; j < suffixesCount; j++)
                    {
                        var expectedNextResult = new DocumentPosition
                        {
                            Document = currentPosition.Document,
                            RowNumber = currentPosition.RowNumber,
                            ColNumber = currentPosition.ColNumber + currentOffset
                        };
                        if (!dictionaries[j].Contains(expectedNextResult))
                        {
                            break;
                        }

                        currentOffset += tokens[j].DistanceToNext;
                        if (j == suffixesCount - 1)
                        {
                            resultList.Add(currentPosition);
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
            var prefixPosition = new DocumentPosition
            {
                ColNumber = startColNumber,
                Document = document,
                RowNumber = rowNumber
            };
            this.AddTerm(term, prefixPosition);
            for (var i = 1; i < term.Length; i++)
            {
                var suffixPosition = new DocumentPosition
                {
                    ColNumber = startColNumber + i,
                    Document = document,
                    RowNumber = rowNumber
                };

                var suffix = term.Substring(i, length - i);
                var prefix = term.Substring(0, i);
                this.AddTerm(suffix, suffixPosition);
                this.AddTerm(prefix, prefixPosition);
            }
        }

        private void AddTerm(string term, DocumentPosition result)
        {
            var hashCode = StringHelper.GetHashCode(term);
            lock (this.syncObj)
            {
                if (!this.dictionary.ContainsKey(hashCode))
                {
                    this.dictionary.Add(hashCode, new HashSet<DocumentPosition> { result });
                }
                else
                {
                    this.dictionary[hashCode].Add(result);
                }
            }
        }
    }
}