using FluentAssertions;
using Indexer.Indexes;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public class InvertedHashIndexTests : BaseIndexTests
    {
        [Test]
        public void Could_Not_Find_Substring_Of_Word()
        {
            var invertedIndex = this.GetNewIndex();

            invertedIndex.Add("test", 0, null);

            invertedIndex.Find("es").Should().BeEmpty();
        }

        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedHashIndex(new DefaultTokenizer());
        }
    }
}