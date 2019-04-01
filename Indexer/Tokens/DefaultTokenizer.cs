using System.Collections.Generic;
using System.Text;

namespace Indexer.Tokens
{
    public class DefaultTokenizer : ITokenizer
    {
        private static readonly List<char> NewTermChars = new List<char> { ' ', '.', '(' };

        public IList<Token> GetTokens(string s)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(s))
            {
                return tokens;
            }

            var term = new StringBuilder();
            var termIndex = 0;
            var previousIsSpace = true;
            var previousToken = new Token();
            for (var i = 0; i < s.Length; i++)
            {
                var c = char.ToLowerInvariant(s[i]);
                if (NewTermChars.Contains(c) && !previousIsSpace)
                {
                    var position = termIndex + 1;
                    previousToken.DistanceToNext = position - previousToken.Position;
                    var token = new Token { Term = term.ToString(), Position = position };
                    tokens.Add(token);
                    previousToken = token;
                    term.Clear();
                    termIndex = i;
                }

                if (c == ' ')
                {
                    termIndex = i + 1;
                    previousIsSpace = true;
                }
                else
                {
                    term.Append(c);
                    previousIsSpace = false;
                }
            }

            if (!previousIsSpace)
            {
                var position = termIndex + 1;
                previousToken.DistanceToNext = position - previousToken.Position;
                tokens.Add(new Token { Term = term.ToString(), Position = position });
            }

            return tokens;
        }
    }
}