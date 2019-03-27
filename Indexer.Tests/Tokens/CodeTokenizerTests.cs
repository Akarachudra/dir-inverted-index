using FluentAssertions;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.Tokens
{
    [TestFixture]
    public class CodeTokenizerTests
    {
        [Test]
        public void Get_Tokens_With_Single_Spaces()
        {
            const string s = "it a test phrase";
            var tokenizer = new CodeTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens[0].Should().BeEquivalentTo(new Token { Term = "it", Position = 1 });
            tokens[1].Should().BeEquivalentTo(new Token { Term = " a", Position = 3 });
            tokens[2].Should().BeEquivalentTo(new Token { Term = " test", Position = 5 });
            tokens[3].Should().BeEquivalentTo(new Token { Term = " phrase", Position = 10 });
        }

        [Test]
        public void Get_Tokens_With_Additional_Spaces()
        {
            const string s = "  et  !.   as ";
            var tokenizer = new CodeTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens[0].Should().BeEquivalentTo(new Token { Term = "  et", Position = 1 });
            tokens[1].Should().BeEquivalentTo(new Token { Term = "  !.", Position = 5 });
            tokens[2].Should().BeEquivalentTo(new Token { Term = "   as", Position = 9 });
            tokens[3].Should().BeEquivalentTo(new Token { Term = " ", Position = 14 });
        }

        [Test]
        public void Terms_Converted_To_Lower_Case()
        {
            const string s = "TeSt";
            var tokenizer = new CodeTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Should().BeEquivalentTo(new Token { Term = "test", Position = 1 });
        }

        [Test]
        public void Tokenizer_Should_Be_Faster_Than_Split()
        {
        }
    }
}