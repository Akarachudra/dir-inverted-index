using FluentAssertions;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.Tokens
{
    [TestFixture]
    public class DefaultTokenizerTests
    {
        [Test]
        public void Get_Tokens_With_Single_Spaces()
        {
            const string s = "it a test phrase";
            var tokenizer = new DefaultTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Count.Should().Be(4);
            tokens[0].Should().BeEquivalentTo(new Token { Term = "it", Position = 1, DistanceToNext = 3 });
            tokens[1].Should().BeEquivalentTo(new Token { Term = "a", Position = 4, DistanceToNext = 2 });
            tokens[2].Should().BeEquivalentTo(new Token { Term = "test", Position = 6, DistanceToNext = 5 });
            tokens[3].Should().BeEquivalentTo(new Token { Term = "phrase", Position = 11, DistanceToNext = 0 });
        }

        [Test]
        public void Get_Tokens_With_Additional_Spaces()
        {
            const string s = "  et  !   as ";
            var tokenizer = new DefaultTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Count.Should().Be(3);
            tokens[0].Should().BeEquivalentTo(new Token { Term = "et", Position = 3, DistanceToNext = 4 });
            tokens[1].Should().BeEquivalentTo(new Token { Term = "!", Position = 7, DistanceToNext = 4 });
            tokens[2].Should().BeEquivalentTo(new Token { Term = "as", Position = 11, DistanceToNext = 0 });
        }

        [Test]
        public void Get_Tokens_With_Dots_And_Brackets()
        {
            const string s = ". m.Get(new);  ";
            var tokenizer = new DefaultTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Count.Should().Be(4);
            tokens[0].Should().BeEquivalentTo(new Token { Term = ".", Position = 1, DistanceToNext = 2 });
            tokens[1].Should().BeEquivalentTo(new Token { Term = "m", Position = 3, DistanceToNext = 1 });
            tokens[2].Should().BeEquivalentTo(new Token { Term = ".get", Position = 4, DistanceToNext = 4 });
            tokens[3].Should().BeEquivalentTo(new Token { Term = "(new);", Position = 8, DistanceToNext = 0 });
        }

        [Test]
        public void Terms_Converted_To_Lower_Case()
        {
            const string s = "TeSt";
            var tokenizer = new DefaultTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Count.Should().Be(1);
            tokens.Should().BeEquivalentTo(new Token { Term = "test", Position = 1, DistanceToNext = 0 });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Return_Empty_List_For_Null_Or_WhiteSpace_String(string s)
        {
            var tokenizer = new DefaultTokenizer();

            var tokens = tokenizer.GetTokens(s);

            tokens.Should().BeEmpty();
        }
    }
}