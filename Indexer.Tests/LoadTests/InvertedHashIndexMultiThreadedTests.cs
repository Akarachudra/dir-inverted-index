using Indexer.Indexes;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.LoadTests
{
    [TestFixture]
    public class InvertedHashIndexMultiThreadedTests : BaseMultiThreadedTests
    {
        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedHashIndex(new DefaultTokenizer());
        }
    }
}