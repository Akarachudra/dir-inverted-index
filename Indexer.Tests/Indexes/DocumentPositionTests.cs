using FluentAssertions;
using Indexer.Indexes;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public class DocumentPositionTests
    {
        [TestCase("test", 2, 1, -1781404647)]
        [TestCase("ok", 4, 6, -261317990)]
        public void HashCode_Test(string pathHash, int rowNumber, int colNumber, int expectedHashCode)
        {
            var documentPosition = new DocumentPosition
            {
                Document = pathHash,
                RowNumber = rowNumber,
                ColNumber = colNumber
            };

            documentPosition.GetHashCode().Should().Be(expectedHashCode);
        }

        [Test]
        public void Equals_Test()
        {
            var firstDocumentPosition = new DocumentPosition
            {
                Document = "Test",
                RowNumber = 10,
                ColNumber = 5
            };
            var secondDocumentPosition = new DocumentPosition
            {
                Document = "Test",
                RowNumber = 10,
                ColNumber = 5
            };

            firstDocumentPosition.Equals(secondDocumentPosition).Should().BeTrue();
            secondDocumentPosition.Document = "New";
            firstDocumentPosition.Equals(secondDocumentPosition).Should().BeFalse();
            secondDocumentPosition.Document = firstDocumentPosition.Document;
            secondDocumentPosition.RowNumber = 25;
            firstDocumentPosition.Equals(secondDocumentPosition).Should().BeFalse();
            secondDocumentPosition.RowNumber = firstDocumentPosition.RowNumber;
            secondDocumentPosition.ColNumber = 25;
            firstDocumentPosition.Equals(secondDocumentPosition).Should().BeFalse();
        }
    }
}