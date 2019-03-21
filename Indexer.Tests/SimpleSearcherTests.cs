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
        private readonly string testFilePath;
        private readonly string pathToFiles;

        public SimpleSearcherTests()
        {
            this.pathToFiles = GetPathToTemplates();
            this.testFilePath = Path.Combine(this.pathToFiles, FirstFileName);
        }

        [SetUp]
        public void RunBeforeTests()
        {
            EnsureErasedFolder(this.pathToFiles);
        }

        [Test]
        public void Can_Find_In_Simple_Single_Line_File()
        {
            const string line = "simple line.";
            File.WriteAllLines(this.testFilePath, new[] { line });
            var simpleSearcher = new SimpleSearcher(this.pathToFiles);

            var results = simpleSearcher.Find("line.");

            results.Count.Should().Be(1);
            var result = results.Single();
            result.RowNumber.Should().Be(1);
            result.ColNumber.Should().Be(8);
            result.FilePath.Should().Be(this.testFilePath);
        }

        [Test]
        public void Can_Find_Several_Matches_In_Simple_Single_Line_File()
        {
            const string line = "simple line. line.";
            File.WriteAllLines(this.testFilePath, new[] { line });
            var simpleSearcher = new SimpleSearcher(this.pathToFiles);
            var firstExpectedResult = new SearchResult
            {
                FilePath = this.testFilePath,
                RowNumber = 1,
                ColNumber = 8
            };
            var secondExpectedResult = new SearchResult
            {
                FilePath = this.testFilePath,
                RowNumber = 1,
                ColNumber = 14
            };

            var results = simpleSearcher.Find("line.");

            results.Should().BeEquivalentTo(firstExpectedResult, secondExpectedResult);
        }

        [Test]
        public void Can_Find_In_Simple_Single_Multiline_File()
        {
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