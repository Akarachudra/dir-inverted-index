using System;
using FluentAssertions;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class PrefixStringComparerTests
    {
        [TestCase("a", "b", -1)]
        [TestCase("b", "a", 1)]
        [TestCase("a", "a", 0)]
        [TestCase("ab", "a", 0)]
        [TestCase("a", "ab", -98)]
        [TestCase("ababa", "aba", 0)]
        public void TestCompares(string x, string y, int expectedResult)
        {
            var comparer = new PrefixStringComparer();

            var result = comparer.Compare(x, y);

            result.Should().Be(expectedResult);
        }

        [TestCase(null, "a")]
        [TestCase("a", null)]
        [TestCase(null, null)]
        public void Throw_ArgumentException_If_Passed_Values_Is_Null(string x, string y)
        {
            var comparer = new PrefixStringComparer();

            Action compareAction = () => comparer.Compare(x, y);

            compareAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Passed string could not be null");
        }
    }
}