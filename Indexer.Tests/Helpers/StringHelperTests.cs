using FluentAssertions;
using Indexer.Helpers;
using NUnit.Framework;

namespace Indexer.Tests.Helpers
{
    [TestFixture]
    public class StringHelperTests
    {
        [TestCase("test", -1701809274)]
        [TestCase("some string", -713055967)]
        public void Can_Calculate_Deterministic_HashCode(string s, int expectedHashCode)
        {
            StringHelper.GetHashCode(s).Should().Be(expectedHashCode);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Return_Zero_HashCode_For_Null_Or_Empty_String(string s)
        {
            StringHelper.GetHashCode(s).Should().Be(0);
        }

        [Test]
        public void Can_Find_All_Indexes_Of_Substring()
        {
            var s = "az i i s giiai";

            var indexes = StringHelper.AllIndexesOf(s, "i");

            indexes.Should().BeEquivalentTo(new[] { 3, 5, 10, 11, 13 }, options => options.WithStrictOrdering());
        }

        [TestCase(null)]
        [TestCase("")]
        public void Return_Empty_Result_For_Passed_Null_Or_Empty_Substring(string substring)
        {
            var s = "str";

            var indexes = StringHelper.AllIndexesOf(s, substring);

            indexes.Should().BeEmpty();
        }
    }
}