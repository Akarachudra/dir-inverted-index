using Indexer.Indexes;
using Indexer.Tests.Base;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public class InvertedHashIndexTests : BaseIndexTests
    {
        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedHashIndex(new DefaultTokenizer());
        }
    }
}