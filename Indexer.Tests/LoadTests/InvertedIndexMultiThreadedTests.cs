using Indexer.Indexes;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.LoadTests
{
    [TestFixture]
    public class InvertedIndexMultiThreadedTests : BaseMultiThreadedTests
    {
        protected override IInvertedIndex GetNewIndex()
        {
            return new InvertedIndex(new DefaultTokenizer());
        }
    }
}