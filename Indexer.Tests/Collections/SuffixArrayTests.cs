using System;
using System.Collections.Generic;
using FluentAssertions;
using Indexer.Collections;
using Moq;
using NUnit.Framework;

namespace Indexer.Tests.Collections
{
    [TestFixture]
    public class SuffixArrayTests
    {
        [Test]
        public void Compare_Via_Insert_Comparer_On_Add()
        {
            var insertComparerMock = new Mock<IComparer<string>>();
            var readComparerMock = new Mock<IComparer<string>>();
            var suffixArray = new SuffixArray<string, int>(insertComparerMock.Object, readComparerMock.Object);

            suffixArray.TryAdd("test", 1);
            suffixArray.TryAdd("tes", 2);

            insertComparerMock.Verify(x => x.Compare("test", "tes"), Times.Once);
            readComparerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Compare_Via_Read_Comparer_On_Get()
        {
            var insertComparerMock = new Mock<IComparer<string>>();
            var readComparerMock = new Mock<IComparer<string>>();
            var suffixArray = new SuffixArray<string, int>(insertComparerMock.Object, readComparerMock.Object);

            suffixArray.TryAdd("test", 1);
            suffixArray.TryAdd("tes", 2);

            suffixArray.TryGetValue("tes", out _);

            readComparerMock.Verify(x => x.Compare("test", "tes"), Times.Once);
        }

        [Test]
        public void Keys_And_Values_Are_Ordered_After_Insert()
        {
            const string word = "ananas";
            var expectedOrderedKeys = new[]
            {
                "ananas", "anas", "as", "nanas", "nas", "s"
            };
            var expectedOrderedValues = new[]
            {
                0, 2, 4, 1, 3, 5
            };
            var suffixArray = new SuffixArray<string, int>(StringComparer.Ordinal, StringComparer.Ordinal);
            for (var i = 0; i < word.Length; i++)
            {
                var suffix = word.Substring(i, word.Length - i);
                suffixArray.TryAdd(suffix, i);
            }

            suffixArray.Keys.Should().BeEquivalentTo(expectedOrderedKeys, options => options.WithStrictOrdering());
            suffixArray.Values.Should().BeEquivalentTo(expectedOrderedValues, options => options.WithStrictOrdering());
        }

        [Test]
        public void Can_Add_And_Get()
        {
            var suffixArray = new SuffixArray<string, int>(StringComparer.Ordinal, StringComparer.Ordinal);
            int value;

            suffixArray.TryAdd("test", 1).Should().BeTrue();
            suffixArray.TryAdd("test", 1).Should().BeFalse();

            suffixArray.TryGetValue("test", out value).Should().BeTrue();
            value.Should().Be(1);
            suffixArray.TryGetValue("not_exists_key", out value).Should().BeFalse();
        }
    }
}