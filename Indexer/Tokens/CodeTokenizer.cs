using System.Collections.Generic;
using System.Text;

namespace Indexer.Tokens
{
    public class CodeTokenizer : ITokenizer
    {
        public IList<Token> GetTokens(string s)
        {
            var tokens = new List<Token>();
            var term = new StringBuilder();
            var termIndex = 0;
            var previousIsSpace = true;
            for (var i = 0; i < s.Length; i++)
            {
                var c = char.ToLowerInvariant(s[i]);
                if (c == ' ' && !previousIsSpace)
                {
                    tokens.Add(new Token { Term = term.ToString(), Position = termIndex + 1 });
                    termIndex = i;
                    term = new StringBuilder();
                }

                term.Append(c);
                previousIsSpace = c == ' ';
            }

            tokens.Add(new Token { Term = term.ToString(), Position = termIndex + 1 });

            return tokens;
        }
    }
}