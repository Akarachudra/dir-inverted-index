using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Indexer.Tests.Base;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class SimpleSearcherTests : TestsBase
    {
        private readonly ISearchService simpleSearcher;

        public SimpleSearcherTests()
        {
            this.simpleSearcher = new SimpleSearcher(this.PathToFiles);
        }

        [Test]
        public void Can_Find_In_Simple_Single_Line_File()
        {
            const string content = "simple line.";
            File.WriteAllLines(this.FirstFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(8);
            result.PathHash.Should().Be(this.FirstFilePath);
        }

        [Test]
        public void Can_Find_Several_Matches_In_Simple_Single_Line_File()
        {
            const string content = "simple line. line.";
            File.WriteAllLines(this.FirstFilePath, new[] { content });
            var firstExpectedResult = new StoredResult
            {
                PathHash = this.FirstFilePath,
                RowNumber = 1,
                ColNumber = 8
            };
            var secondExpectedResult = new StoredResult
            {
                PathHash = this.FirstFilePath,
                RowNumber = 1,
                ColNumber = 14
            };

            var results = this.simpleSearcher.Find("line.");

            results.Should().BeEquivalentTo(firstExpectedResult, secondExpectedResult);
        }

        [Test]
        public void Can_Find_In_Simple_Single_Multiline_File()
        {
            var content = $"simple {Environment.NewLine}new line.";
            File.WriteAllLines(this.FirstFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(2);
            result.ColNumber.Should().Be(5);
            result.PathHash.Should().Be(this.FirstFilePath);
        }

        [Test]
        public void Can_Find_In_Several_Files()
        {
            const string firstFileContent = "first line.";
            var secondFileContent = $"{Environment.NewLine}new line.";
            File.WriteAllLines(this.FirstFilePath, new[] { firstFileContent });
            File.WriteAllLines(this.SecondFilePath, new[] { secondFileContent });
            var firstExpectedResult = new StoredResult
            {
                PathHash = this.FirstFilePath,
                RowNumber = 1,
                ColNumber = 7
            };
            var secondExpectedResult = new StoredResult
            {
                PathHash = this.SecondFilePath,
                RowNumber = 2,
                ColNumber = 5
            };

            var results = this.simpleSearcher.Find("line.");

            results.Should().BeEquivalentTo(firstExpectedResult, secondExpectedResult);
        }

        [Test]
        public void Can_Find_In_Included_File()
        {
            const string content = "first line.";
            EnsureErasedFolder(this.IncludedDirPath);
            File.WriteAllLines(this.IncludedFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(7);
            result.PathHash.Should().Be(this.IncludedFilePath);
        }

        [Test]
        public void Can_Find_In_Deep_Included_File()
        {
            const string content = "first line.";
            EnsureErasedFolder(this.IncludedDirPath);
            EnsureErasedFolder(this.DeepIncludedDirPath);
            File.WriteAllLines(this.DeepIncludedFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(7);
            result.PathHash.Should().Be(this.DeepIncludedFilePath);
        }

        [Test]
        public void SimpleSearcher_Is_Case_Insensitive()
        {
            const string content = "simple LiNe.";
            File.WriteAllLines(this.FirstFilePath, new[] { content });

            var results = this.simpleSearcher.Find("lIne.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(8);
            result.PathHash.Should().Be(this.FirstFilePath);
        }
    }
}