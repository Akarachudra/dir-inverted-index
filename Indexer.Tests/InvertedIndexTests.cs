using FluentAssertions;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class InvertedIndexTests
    {
        [Test]
        public void Can_Add_To_Index_And_Find_Matches()
        {
            const string pathHash = "hash";
            const string word = "te_st";
            var invertedIndex = new InvertedIndex(new CodeTokenizer());
            var searchResult = new StoredResult { RowNumber = 2, ColNumber = 1, PathHash = pathHash };

            invertedIndex.Add(word, 2, pathHash);

            invertedIndex.Find(word).Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("te_s").Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("e_st").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 2, PathHash = pathHash });
            invertedIndex.Find("e_s").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 2, PathHash = pathHash });
            invertedIndex.Find("some_word").Should().BeEmpty();
        }

        [Test]
        public void InvertedIndex_Is_Case_Insensitive()
        {
            var invertedIndex = new InvertedIndex(new CodeTokenizer());
            var expectedResult = new StoredResult { RowNumber = 2, ColNumber = 1, PathHash = "path" };

            invertedIndex.Add("T_e_s_T", expectedResult.RowNumber, expectedResult.PathHash);

            invertedIndex.Find("t_E_s_t").Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Can_Find_Simple_Phrase()
        {
            const string simplePhrase = "some phrase in code";
            var invertedIndex = new InvertedIndex(new CodeTokenizer());
            InsertPhraseToIndex(invertedIndex, simplePhrase);

            invertedIndex.Find("some phrase").Should().BeEquivalentTo(new StoredResult { ColNumber = 1 });
            invertedIndex.Find("ome phr").Should().BeEquivalentTo(new StoredResult { ColNumber = 2 });
            invertedIndex.Find("ase in cod").Should().BeEquivalentTo(new StoredResult { ColNumber = 9 });
        }

        [Test]
        public void Can_Find_Word_With_Space()
        {
            const string phrase = "t est";
            var invertedIndex = new InvertedIndex(new CodeTokenizer());
            InsertPhraseToIndex(invertedIndex, phrase);

            invertedIndex.Find(" est").Should().BeEquivalentTo(new StoredResult { ColNumber = 2 });
        }

        [Test]
        public void Can_Find_Phrase_With_Additional_Spaces_And_Symbols()
        {
            const string additionalPhrase = "As.  phrase   ! ";
            var invertedIndex = new InvertedIndex(new CodeTokenizer());
            InsertPhraseToIndex(invertedIndex, additionalPhrase);

            invertedIndex.Find("as").Should().BeEquivalentTo(new StoredResult { ColNumber = 1 }, new StoredResult { ColNumber = 9 });
            invertedIndex.Find("As.  ").Should().BeEquivalentTo(new StoredResult { ColNumber = 1 });
            invertedIndex.Find("rase   ").Should().BeEquivalentTo(new StoredResult { ColNumber = 8 });
            invertedIndex.Find("  phrase   ").Should().BeEquivalentTo(new StoredResult { ColNumber = 4 });
            invertedIndex.Find(" !").Should().BeEquivalentTo(new StoredResult { ColNumber = 14 });
            invertedIndex.Find("!").Should().BeEquivalentTo(new StoredResult { ColNumber = 15 });
            invertedIndex.Find("! ").Should().BeEquivalentTo(new StoredResult { ColNumber = 15 });
        }

        private static void InsertPhraseToIndex(InvertedIndex index, string phrase, string pathHash = null)
        {
            index.Add(phrase, 0, pathHash);
        }
    }
}