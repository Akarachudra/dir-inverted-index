﻿using FluentAssertions;
using Indexer.Indexes;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public class StoredResultTests
    {
        [TestCase("test", 2, 1, -1781404647)]
        [TestCase("ok", 4, 6, -261317990)]
        public void HashCode_Test(string pathHash, int rowNumber, int colNumber, int expectedHashCode)
        {
            var storedResult = new StoredResult
            {
                Document = pathHash,
                RowNumber = rowNumber,
                ColNumber = colNumber
            };

            storedResult.GetHashCode().Should().Be(expectedHashCode);
        }

        [Test]
        public void Equals_Test()
        {
            var firstStoredResult = new StoredResult
            {
                Document = "Test",
                RowNumber = 10,
                ColNumber = 5
            };
            var secondStoredResult = new StoredResult
            {
                Document = "Test",
                RowNumber = 10,
                ColNumber = 5
            };

            firstStoredResult.Equals(secondStoredResult).Should().BeTrue();
            secondStoredResult.Document = "New";
            firstStoredResult.Equals(secondStoredResult).Should().BeFalse();
            secondStoredResult.Document = firstStoredResult.Document;
            secondStoredResult.RowNumber = 25;
            firstStoredResult.Equals(secondStoredResult).Should().BeFalse();
            secondStoredResult.RowNumber = firstStoredResult.RowNumber;
            secondStoredResult.ColNumber = 25;
            firstStoredResult.Equals(secondStoredResult).Should().BeFalse();
        }
    }
}