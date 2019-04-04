using System;
using System.Collections.Generic;
using FluentAssertions;
using Indexer.Indexes;
using NUnit.Framework;

namespace Indexer.Tests.Indexes
{
    [TestFixture]
    public abstract class BaseIndexTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Passed_Document_Could_Not_Be_Null_Or_WhiteSpace(string document)
        {
            var invertedIndex = this.GetNewIndex();

            Action addAction = () => invertedIndex.Add(new[] { "line" }, document);

            addAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Document could not be null or whitespace");
        }

        [Test]
        public void Can_Add_And_Find_Single_Match()
        {
            const string document = "doc";
            const string word = "te_st";
            var invertedIndex = this.GetNewIndex();
            var expectedResult = new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document };

            invertedIndex.Add(new[] { word }, document);

            invertedIndex.Find(word).Should().BeEquivalentTo(expectedResult);
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
            var expectedResult = new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document };

            invertedIndex.Add(new[] { word }, document);

            invertedIndex.Find("c").Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Can_Find_By_Prefixes_And_Suffixes()
        {
            const string document = "doc";
            const string word = "te_st";
            var invertedIndex = this.GetNewIndex();
            var expectedResult = new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document };

            invertedIndex.Add(new[] { word }, document);

            invertedIndex.Find("te_s").Should().BeEquivalentTo(expectedResult);
            invertedIndex.Find("te_").Should().BeEquivalentTo(expectedResult);
            invertedIndex.Find("te").Should().BeEquivalentTo(expectedResult);
            invertedIndex.Find("e_st").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 2, Document = document });
            invertedIndex.Find("_st").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 3, Document = document });
            invertedIndex.Find("st").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 4, Document = document });
            invertedIndex.Find("t").Should().BeEquivalentTo(expectedResult, new DocumentPosition { RowNumber = 1, ColNumber = 5, Document = document });
        }

        [Test]
        public void Can_Find_Simple_Phrase()
        {
            const string document = "doc";
            const string simplePhrase = "some phr.se !n code";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { simplePhrase }, document);

            invertedIndex.Find("some phr.se").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = document });
            invertedIndex.Find("ome phr").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 2, Document = document });
            invertedIndex.Find(".se !n cod").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 9, Document = document });
        }

        [Test]
        public void Can_Find_With_Several_Spaces_In_Middle_At_Phrase()
        {
            const string document = "doc";
            const string simplePhrase = "some   phrase";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { simplePhrase }, document);

            invertedIndex.Find("ome   phra").Should().BeEquivalentTo(new DocumentPosition { RowNumber = 1, ColNumber = 2, Document = document });
            invertedIndex.Find("ome  phr").Should().BeEmpty();
        }

        [Test]
        public void Start_And_End_Spaces_Are_Ignored()
        {
            const string document = "doc";
            const string word = "  test  ";
            var invertedIndex = this.GetNewIndex();
            var expectedResults = new List<DocumentPosition>
            {
                new DocumentPosition { RowNumber = 1, ColNumber = 3, Document = document },
                new DocumentPosition { RowNumber = 1, ColNumber = 6, Document = document }
            };
            invertedIndex.Add(new[] { word }, document);

            invertedIndex.Find("  t").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find("t  ").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find("t ").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find(" t").Should().BeEquivalentTo(expectedResults);
            invertedIndex.Find(" ").Should().BeEmpty();
        }

        [Test]
        public void Queries_With_Spaces_Only_Are_Ignored()
        {
            const string document = "doc";
            const string word = "   test   ";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { word }, document);

            invertedIndex.Find(" ").Should().BeEmpty();
            invertedIndex.Find("  ").Should().BeEmpty();
            invertedIndex.Find("   ").Should().BeEmpty();
        }

        [Test]
        public void Null_And_Empty_Queries_Are_Ignored()
        {
            const string document = "doc";
            const string word = "   test   ";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { word }, document);

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
            invertedIndex.Add(new[] { firstPhrase }, firstPhrase);
            invertedIndex.Add(new[] { secondPhrase }, secondPhrase);
            invertedIndex.Add(new[] { thirdPhrase }, thirdPhrase);

            invertedIndex.Find("some program int")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = firstPhrase },
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = secondPhrase },
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = thirdPhrase });
        }

        [Test]
        public void Last_Suffix_Is_Not_Ignored()
        {
            const string document = "doc";
            const string phrase = "some program interface";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { phrase }, document);

            invertedIndex.Find("some program notinterface").Should().BeEmpty();
        }

        [Test]
        public void Can_Find_Several_Words_With_Single_Prefix()
        {
            const string firstWord = "someword";
            const string secondWord = "someanotherword";
            const string anotherDocumentWord = "someanotherdocumentword";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { firstWord, secondWord }, "doc1");
            invertedIndex.Add(new[] { anotherDocumentWord }, "doc2");

            invertedIndex.Find("some")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = "doc1" },
                             new DocumentPosition { RowNumber = 2, ColNumber = 1, Document = "doc1" },
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = "doc2" });
        }

        [Test]
        public void Same_Phrase_With_Same_Row_In_Different_Documents()
        {
            const string firstPhrase = "some program";
            const string secondPhrase = "zzzz program";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { firstPhrase, secondPhrase }, "doc1");
            invertedIndex.Add(new[] { firstPhrase, secondPhrase }, "doc2");

            invertedIndex.Find("some program")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = "doc1" },
                             new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = "doc2" });
        }

        [Test]
        public void Index_Is_Case_Insensitive()
        {
            var invertedIndex = this.GetNewIndex();
            var expectedResult = new DocumentPosition { RowNumber = 1, ColNumber = 1, Document = "path" };

            invertedIndex.Add(new[] { "T_e_s_T" }, expectedResult.Document);

            invertedIndex.Find("t_E_s_t").Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Same_Suffixes_For_First_Term()
        {
            const string firstPhrase = "some program";
            const string secondPhrase = "awesome program";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { firstPhrase, secondPhrase }, "doc1");

            invertedIndex.Find("ome program")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 2, Document = "doc1" },
                             new DocumentPosition { RowNumber = 2, ColNumber = 5, Document = "doc1" });
        }

        [Test]
        public void Space_Wrappers_Are_Ignored()
        {
            const string firstPhrase = "   some program";
            const string secondPhrase = "awesome program ";
            var invertedIndex = this.GetNewIndex();
            invertedIndex.Add(new[] { firstPhrase, secondPhrase }, "doc1");

            invertedIndex.Find(" some program   ")
                         .Should()
                         .BeEquivalentTo(
                             new DocumentPosition { RowNumber = 1, ColNumber = 4, Document = "doc1" },
                             new DocumentPosition { RowNumber = 2, ColNumber = 4, Document = "doc1" });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Return_Empty_Result_If_Phrase_Is_Null_Or_Whitespaces(string phrase)
        {
            var invertedIndex = this.GetNewIndex();

            invertedIndex.Find(phrase)
                         .Should()
                         .BeEmpty();
        }

        protected abstract IInvertedIndex GetNewIndex();
    }
}