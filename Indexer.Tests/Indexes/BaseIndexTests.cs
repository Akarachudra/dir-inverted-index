using System.Collections.Generic;
using FluentAssertions;
using Indexer.Indexes;
using Indexer.Tests.Base;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public abstract class BaseIndexTests : TestsBase
    {
        [Test]
        public void Can_Add_And_Find_Single_Match()
        {
            const string document = "doc";
            const string word = "te_st";
            var invertedIndex = this.GetNewIndex();
            var searchResult = new StoredResult { RowNumber = 2, ColNumber = 1, Document = document };

            invertedIndex.Add(word, 2, document);

            invertedIndex.Find(word).Should().BeEquivalentTo(searchResult);
        }

        [Test]
        public void Return_Empty_Result_For_No_Matches()
        {
            var invertedIndex = this.GetNewIndex();

            invertedIndex.Find("some word").Should().BeEmpty();
        }

        [Test]
        public void Can_Find_Single_Char_Word()
        {
            const string document = "doc";
            const string word = "c";
            var invertedIndex = this.GetNewIndex();
            var searchResult = new StoredResult { RowNumber = 2, ColNumber = 1, Document = document };

            invertedIndex.Add(word, 2, document);

            invertedIndex.Find("c").Should().BeEquivalentTo(searchResult);
        }

        [Test]
        public void Can_Find_By_Prefixes_And_Suffixes()
        {
            const string document = "doc";
            const string word = "te_st";
            var invertedIndex = this.GetNewIndex();
            var searchResult = new StoredResult { RowNumber = 2, ColNumber = 1, Document = document };

            invertedIndex.Add(word, 2, document);

            invertedIndex.Find("te_s").Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("te_").Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("te").Should().BeEquivalentTo(searchResult);
            invertedIndex.Find("e_st").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 2, Document = document });
            invertedIndex.Find("_st").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 3, Document = document });
            invertedIndex.Find("st").Should().BeEquivalentTo(new StoredResult { RowNumber = 2, ColNumber = 4, Document = document });
            invertedIndex.Find("t").Should().BeEquivalentTo(searchResult, new StoredResult { RowNumber = 2, ColNumber = 5, Document = document });
        }

        [Test]
        public void Can_Find_Simple_Phrase()
        {
            const string simplePhrase = "some phr.se !n code";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(simplePhrase, 0, null);

            invertedIndex.Find("some phr.se").Should().BeEquivalentTo(new StoredResult { ColNumber = 1 });
            invertedIndex.Find("ome phr").Should().BeEquivalentTo(new StoredResult { ColNumber = 2 });
            invertedIndex.Find(".se !n cod").Should().BeEquivalentTo(new StoredResult { ColNumber = 9 });
        }

        [Test]
        public void Can_Find_With_Several_Spaces_In_Middle_At_Phrase()
        {
            const string simplePhrase = "some   phrase";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(simplePhrase, 0, null);

            invertedIndex.Find("ome   phra").Should().BeEquivalentTo(new StoredResult { ColNumber = 2 });
            invertedIndex.Find("ome  phr").Should().BeEmpty();
        }

        [Test]
        public void Start_And_End_Spaces_Are_Ignored()
        {
            const string word = "  test  ";
            var invertedIndex = this.GetNewIndex();
            var expectedResults = new List<StoredResult> { new StoredResult { ColNumber = 3 }, new StoredResult { ColNumber = 6 } };
            invertedIndex.Add(word, 0, null);

            invertedIndex.Find("  t").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find("t  ").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find("t ").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find(" t").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find(" ").Should().BeEmpty();
        }

        [Test]
        public void Queries_With_Spaces_Only_Are_Ignored()
        {
            const string word = "   test   ";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(word, 0, null);

            invertedIndex.Find(" ").Should().BeEmpty();
            invertedIndex.Find("  ").Should().BeEmpty();
            invertedIndex.Find("   ").Should().BeEmpty();
        }

        [Test]
        public void Null_And_Empty_Queries_Are_Ignored()
        {
            const string word = "   test   ";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(word, 0, null);

            invertedIndex.Find(string.Empty).Should().BeEmpty();
            invertedIndex.Find(null).Should().BeEmpty();
        }

        [Test]
        public void Can_Find_All_Matches_With_Same_Suffixes()
        {
            const string firstPhrase = "some program interface";
            const string secondPhrase = "some program int";
            const string thirdPhrase = "some program int32";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(firstPhrase, 0, firstPhrase);
            invertedIndex.Add(secondPhrase, 0, secondPhrase);
            invertedIndex.Add(thirdPhrase, 0, thirdPhrase);

            invertedIndex.Find("some program int")
                         .Should()
                         .BeEquivalentTo(
                             new StoredResult { ColNumber = 1, Document = firstPhrase },
                             new StoredResult { ColNumber = 1, Document = secondPhrase },
                             new StoredResult { ColNumber = 1, Document = thirdPhrase });
        }

        [Test]
        public void Last_Suffix_Is_Not_Ignored()
        {
            const string phrase = "some program interface";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(phrase, 0, null);

            invertedIndex.Find("some program notinterface").Should().BeEmpty();
        }

        [Test]
        public void Can_Find_Several_Words_With_Single_Prefix()
        {
            const string firstWord = "someword";
            const string secondWord = "someanotherword";
            const string anotherDocumentWord = "someanotherdocumentword";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(secondWord, 0, "doc1");
            invertedIndex.Add(firstWord, 1, "doc1");
            invertedIndex.Add(anotherDocumentWord, 2, "doc2");

            invertedIndex.Find("some")
                         .Should()
                         .BeEquivalentTo(
                             new StoredResult { ColNumber = 1, Document = "doc1" },
                             new StoredResult { ColNumber = 1, RowNumber = 1, Document = "doc1" },
                             new StoredResult { ColNumber = 1, RowNumber = 2, Document = "doc2" });
        }

        [Test]
        public void Index_Is_Case_Insensitive()
        {
            var invertedIndex = this.GetNewIndex();
            var expectedResult = new StoredResult { RowNumber = 2, ColNumber = 1, Document = "path" };

            invertedIndex.Add("T_e_s_T", expectedResult.RowNumber, expectedResult.Document);

            invertedIndex.Find("t_E_s_t").Should().BeEquivalentTo(expectedResult);
        }

        protected abstract IInvertedIndex GetNewIndex();
    }
}