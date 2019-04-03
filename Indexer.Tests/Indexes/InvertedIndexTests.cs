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
            const string document = "doc";
            const string additionalPhrase = "As.  phrase   ! ";
            var invertedIndex = new InvertedIndex(new DefaultTokenizer());
            invertedIndex.Add(new[] { additionalPhrase }, document);

            invertedIndex.Find("as")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document },
                             new DocumentPosition { RowNumber = 1, ColNumber = 9, Document = document });
            invertedIndex.Find("As.  ").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document });
            invertedIndex.Find("rase   ").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 8, Document = document });
            invertedIndex.Find("  phrase   ").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 6, Document = document });
            invertedIndex.Find(" !").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 15, Document = document });
            invertedIndex.Find("!").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 15, Document = document });
            invertedIndex.Find("! ").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 15, Document = document });
        }

        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedIndex(new DefaultTokenizer());
        }
    }
}