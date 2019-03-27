using FluentAssertions;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class InvertedIndexTests
    {
        [Test]
        public void Can_Add_To_Index_And_Find_Matches()
        {
            const string path = "file_path";
            const string word = "te_st";
            var invertedIndex = new InvertedIndex();
            var searchResult = new SearchResult { RowNumber = 2, ColNumber = 1, FilePath = path };

            invertedIndex.Add(word, searchResult);

            invertedIndex.Find(word).Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("te_s").Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("e_st").Should().BeEquivalentTo(new SearchResult { RowNumber = 2, ColNumber = 2, FilePath = path });
            invertedIndex.Find("e_s").Should().BeEquivalentTo(new SearchResult { RowNumber = 2, ColNumber = 2, FilePath = path });
            invertedIndex.Find("some_word").Should().BeEmpty();
        }

        [Test]
        public void InvertedIndex_Is_Case_Insensitive()
        {
            var invertedIndex = new InvertedIndex();
            var searchResult = new SearchResult { RowNumber = 2, ColNumber = 1, FilePath = "path" };

            invertedIndex.Add("T_e_s_T", searchResult);

            invertedIndex.Find("t_E_s_t").Should().BeEquivalentTo(searchResult);
        }

        [Test]
        public void Can_Find_Simple_Phrase()
        {
            const string file = "file";
            var invertedIndex = new InvertedIndex();

            invertedIndex.Add("some", new SearchResult { FilePath = file, ColNumber = 1, RowNumber = 1 });
            invertedIndex.Add("phrase", new SearchResult { FilePath = file, ColNumber = 6, RowNumber = 1 });
            invertedIndex.Add("in", new SearchResult { FilePath = file, ColNumber = 13, RowNumber = 1 });
            invertedIndex.Add("code", new SearchResult { FilePath = file, ColNumber = 16, RowNumber = 1 });

            invertedIndex.Find("some phrase").Should().BeEquivalentTo(new SearchResult { FilePath = file, ColNumber = 1, RowNumber = 1 });
            invertedIndex.Find("ome phr").Should().BeEquivalentTo(new SearchResult { FilePath = file, ColNumber = 2, RowNumber = 1 });
            invertedIndex.Find("ase in cod").Should().BeEquivalentTo(new SearchResult { FilePath = file, ColNumber = 9, RowNumber = 1 });
        }
    }
}