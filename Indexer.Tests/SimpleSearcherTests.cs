using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class SimpleSearcherTests
    {
        private const string TestFilesFolder = @"Test_Data";
        private const string FirstFileName = "FirstFileName.txt";
        private const string SecondFileName = "SecondFileName.txt";
        private const string IncludedFileName = "IncludedFileName.txt";
        private const string DeepIncludedFileName = "DeepIncludedFileName.txt";
        private const string IncludedDir = "IncludedDir";
        private const string DeepIncludedDir = "DeepIncludedDir";
        private readonly string firstFilePath;
        private readonly string secondFilePath;
        private readonly string pathToFiles;
        private readonly ISearchService simpleSearcher;

        public SimpleSearcherTests()
        {
            this.pathToFiles = GetPathToTemplates();
            this.firstFilePath = Path.Combine(this.pathToFiles, FirstFileName);
            this.secondFilePath = Path.Combine(this.pathToFiles, SecondFileName);
            this.simpleSearcher = new SimpleSearcher(this.pathToFiles);
        }

        [SetUp]
        public void RunBeforeTests()
        {
            EnsureErasedFolder(this.pathToFiles);
        }

        [Test]
        public void Can_Find_In_Simple_Single_Line_File()
        {
            const string content = "simple line.";
            File.WriteAllLines(this.firstFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(8);
            result.FilePath.Should().Be(this.firstFilePath);
        }

        [Test]
        public void Can_Find_Several_Matches_In_Simple_Single_Line_File()
        {
            const string content = "simple line. line.";
            File.WriteAllLines(this.firstFilePath, new[] { content });
            var firstExpectedResult = new SearchResult
            {
                FilePath = this.firstFilePath,
                RowNumber = 1,
                ColNumber = 8
            };
            var secondExpectedResult = new SearchResult
            {
                FilePath = this.firstFilePath,
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
            File.WriteAllLines(this.firstFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(2);
            result.ColNumber.Should().Be(5);
            result.FilePath.Should().Be(this.firstFilePath);
        }

        [Test]
        public void Can_Find_In_Several_Files()
        {
            const string firstFileContent = "first line.";
            var secondFileContent = $"{Environment.NewLine}new line.";
            File.WriteAllLines(this.firstFilePath, new[] { firstFileContent });
            File.WriteAllLines(this.secondFilePath, new[] { secondFileContent });
            var firstExpectedResult = new SearchResult
            {
                FilePath = this.firstFilePath,
                RowNumber = 1,
                ColNumber = 7
            };
            var secondExpectedResult = new SearchResult
            {
                FilePath = this.secondFilePath,
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
            var directoryPath = Path.Combine(this.pathToFiles, IncludedDir);
            Directory.CreateDirectory(directoryPath);
            var includedFilePath = Path.Combine(directoryPath, IncludedFileName);
            File.WriteAllLines(includedFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(7);
            result.FilePath.Should().Be(includedFilePath);
        }

        [Test]
        public void Can_Find_In_Deep_Included_File()
        {
            const string content = "first line.";
            var directoryPath = Path.Combine(this.pathToFiles, IncludedDir);
            Directory.CreateDirectory(directoryPath);
            var deepPath = Path.Combine(directoryPath, DeepIncludedDir);
            Directory.CreateDirectory(deepPath);
            var deepIncludedFilePath = Path.Combine(deepPath, DeepIncludedFileName);
            File.WriteAllLines(deepIncludedFilePath, new[] { content });

            var results = this.simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(7);
            result.FilePath.Should().Be(deepIncludedFilePath);
        }

        private static string GetPathToTemplates()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, TestFilesFolder);
        }

        private static void EnsureErasedFolder(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            Directory.CreateDirectory(path);
        }
    }
}