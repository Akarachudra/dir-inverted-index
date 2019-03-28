using FluentAssertions;
using Indexer.Indexes;
using Indexer.SuffixTrie;
using NUnit.Framework;

namespace Indexer.Tests.SuffixTrie
{
    [TestFixture]
    public class ConcurrentSuffixTrieTests
    {
        [Test]
        public void Can_Insert_Word_And_Read_By_Suffix()
        {
            const string document = "doc";
            const string suffix = "te_st";
            var trie = new ConcurrentSuffixTrie();
            var searchResult = new StoredResult { RowNumber = 2, ColNumber = 1, Document = document };

            trie.Insert(suffix, searchResult);

            trie.Find(suffix).Should().BeEquivalentTo(searchResult);
            trie.Find("te_s").Should().BeEquivalentTo(searchResult);
            trie.Find("e_st").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 2, Document = document });
            trie.Find("e_s").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 2, Document = document });
            trie.Find("some_word").Should().BeEmpty();
        }
    }
}