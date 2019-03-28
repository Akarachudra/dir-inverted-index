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
    }
}