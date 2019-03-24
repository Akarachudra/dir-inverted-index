using FluentAssertions;
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
            const string path = "file_path";
            const string suffix = "te_st";
            var trie = new ConcurrentSuffixTrie();
            var searchResult = new SearchResult { RowNumber = 2, ColNumber = 1, FilePath = path };

            trie.Insert(suffix, searchResult);

            trie.Find(suffix).Should().BeEquivalentTo(searchResult);
            trie.Find("te_s").Should().BeEquivalentTo(searchResult);
            trie.Find("e_st").Should().BeEquivalentTo(new SearchResult { RowNumber = 2, ColNumber = 2, FilePath = path });
            trie.Find("e_s").Should().BeEquivalentTo(new SearchResult { RowNumber = 2, ColNumber = 2, FilePath = path });
            trie.Find("some_word").Should().BeEmpty();
        }
    }
}