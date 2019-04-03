using FluentAssertions;
using Indexer.Indexes;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public class InvertedIndexTests : BaseIndexTests
    {
        [Test]
        public void Can_Find_Phrase_With_Additional_Spaces_And_Symbols()
        {
            const string additionalPhrase = "As.  phrase   ! ";
            var invertedIndex = new InvertedIndex(new DefaultTokenizer());
            invertedIndex.Add(additionalPhrase, 0, null);

            invertedIndex.Find("as").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 1 }, new DocumentPosition { ColNumber = 9 });
            invertedIndex.Find("As.  ").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 1 });
            invertedIndex.Find("rase   ").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 8 });
            invertedIndex.Find("  phrase   ").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 6 });
            invertedIndex.Find(" !").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 15 });
            invertedIndex.Find("!").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 15 });
            invertedIndex.Find("! ").Should().BeEquivalentTo(new DocumentPosition { ColNumber = 15 });
        }

        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedIndex(new DefaultTokenizer());
        }
    }
}