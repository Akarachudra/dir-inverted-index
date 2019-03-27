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
            const string simplePhrase = "some phrase in code";
            var invertedIndex = new InvertedIndex();
            InsertPhraseToIndex(invertedIndex, simplePhrase);

            invertedIndex.Find("some phrase").Should().BeEquivalentTo(new SearchResult { ColNumber = 1 });
            invertedIndex.Find("ome phr").Should().BeEquivalentTo(new SearchResult { ColNumber = 2 });
            invertedIndex.Find("ase in cod").Should().BeEquivalentTo(new SearchResult { ColNumber = 9 });
        }

        [Test]
        public void Can_Find_Word_With_Space()
        {
            const string phrase = "t est";
            var invertedIndex = new InvertedIndex();
            InsertPhraseToIndex(invertedIndex, phrase);

            invertedIndex.Find(" est").Should().BeEquivalentTo(new SearchResult { ColNumber = 2 });
        }

        [Test]
        public void Can_Find_Phrase_With_Additional_Spaces_And_Symbols()
        {
            const string additionalPhrase = "Ad.  phrase   ! ";
            var invertedIndex = new InvertedIndex();
            InsertPhraseToIndex(invertedIndex, additionalPhrase);

            invertedIndex.Find("Ad.  ").Should().BeEquivalentTo(new SearchResult { ColNumber = 1 });
            invertedIndex.Find("rase   ").Should().BeEquivalentTo(new SearchResult { ColNumber = 8 });
            invertedIndex.Find("  phrase   ").Should().BeEquivalentTo(new SearchResult { ColNumber = 4 });
            invertedIndex.Find(" !").Should().BeEquivalentTo(new SearchResult { ColNumber = 14 });
            invertedIndex.Find("!").Should().BeEquivalentTo(new SearchResult { ColNumber = 15 });
            invertedIndex.Find("! ").Should().BeEquivalentTo(new SearchResult { ColNumber = 15 });
        }

        private static void InsertPhraseToIndex(InvertedIndex index, string phrase, string filePath = null)
        {
            var terms = phrase.Split(' ');
            var offset = 1;
            foreach (var term in terms)
            {
                var curResult = new SearchResult
                {
                    FilePath = filePath,
                    ColNumber = offset
                };
                index.Add(term, curResult);
                offset += term.Length + 1;
            }
        }
    }
}