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
            var suffixArray = new SuffixArray<string, int>();

            suffixArray.TryAdd("test", 1, insertComparerMock.Object);
            suffixArray.TryAdd("tes", 2, insertComparerMock.Object);

            insertComparerMock.Verify(x => x.Compare("test", "tes"), Times.Once);
        }

        [Test]
        public void Compare_Via_Read_Comparer_On_Get()
        {
            var insertComparerMock = new Mock<IComparer<string>>();
            var readComparerMock = new Mock<IComparer<string>>();
            var suffixArray = new SuffixArray<string, int>();

            suffixArray.TryAdd("test", 1, insertComparerMock.Object);
            suffixArray.TryAdd("tes", 2, insertComparerMock.Object);

            suffixArray.TryGetValue("tes", out _, readComparerMock.Object);

            readComparerMock.Verify(x => x.Compare("test", "tes"), Times.Once);
        }

        [Test]
        public void Keys_And_Values_Are_Ordered_After_Insert()
        {
            var stringComparer = StringComparer.Ordinal;
            const string word = "ananas";
            var expectedOrderedKeys = new[]
            {
                "ananas", "anas", "as", "nanas", "nas", "s"
            };
            var expectedOrderedValues = new[]
            {
                0, 2, 4, 1, 3, 5
            };
            var suffixArray = new SuffixArray<string, int>();
            for (var i = 0; i < word.Length; i++)
            {
                var suffix = word.Substring(i, word.Length - i);
                suffixArray.TryAdd(suffix, i, stringComparer);
            }

            suffixArray.Keys.Should().BeEquivalentTo(expectedOrderedKeys, options => options.WithStrictOrdering());
            suffixArray.Values.Should().BeEquivalentTo(expectedOrderedValues, options => options.WithStrictOrdering());
        }

        [Test]
        public void Can_Add_And_Get()
        {
            var stringComparer = StringComparer.Ordinal;
            var suffixArray = new SuffixArray<string, int>();
            int value;

            suffixArray.TryAdd("test", 1, stringComparer).Should().BeTrue();
            suffixArray.TryAdd("test", 1, stringComparer).Should().BeFalse();

            suffixArray.TryGetValue("test", out value, stringComparer).Should().BeTrue();
            value.Should().Be(1);
            suffixArray.TryGetValue("not_exists_key", out value, stringComparer).Should().BeFalse();
        }

        [Test]
        public void Check_Count_And_Capacity()
        {
            var intComparer = Comparer<int>.Default;
            var suffixArray = new SuffixArray<int, int>();

            suffixArray.TryAdd(1, 1, intComparer);
            suffixArray.Count.Should().Be(1);
            suffixArray.Capacity.Should().Be(16);

            for (var i = 0; i < 16; i++)
            {
                suffixArray.TryAdd(i + 2, i, intComparer);
            }

            suffixArray.Count.Should().Be(17);
            suffixArray.Capacity.Should().Be(32);
        }
    }
}