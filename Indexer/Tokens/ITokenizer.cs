using System.Collections.Generic;

namespace Indexer.Tokens
{
    public interface ITokenizer
    {
        IList<Token> GetTokens(string s);
    }
}