using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Indexer.Indexes;
using Indexer.Tests.Base;
using Indexer.Tokens;
using Indexer.Watch;
using NUnit.Framework;

namespace Indexer.Tests.LoadTests
{
    [TestFixture]
    public class IndexServiceBenchmarkTests : TestsBase
    {
        private const string FirstFileLastLine = "@#$firstlastline";
        private const string SecondFileLastLine = "@#$secondlastline";
        private const string IncludedFileLastLine = "@#$includedlastline";
        private const string DeepIncludedFileLastLine = "@#$deepincludedlastline";

        public override void RunBeforeTests()
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            const int fromTicks = 200;
            var firstFileLines = TestDataGenerator.GetRandomLines(fromTicks, 30000);
            var secondFileLines = TestDataGenerator.GetRandomLines(fromTicks, 30000);
            var includedFileLines = TestDataGenerator.GetRandomLines(fromTicks, 30000);
            var deepIncludedFileLines = TestDataGenerator.GetRandomLines(fromTicks, 30000);
            EnsureErasedFolder(this.PathToFiles);
            EnsureErasedFolder(this.IncludedDirPath);
            EnsureErasedFolder(this.DeepIncludedDirPath);
            File.WriteAllLines(this.FirstFilePath, firstFileLines);
            File.AppendAllLines(this.FirstFilePath, new[] { FirstFileLastLine });
            File.WriteAllLines(this.SecondFilePath, secondFileLines);
            File.AppendAllLines(this.SecondFilePath, new[] { SecondFileLastLine });
            File.WriteAllLines(this.IncludedFilePath, includedFileLines);
            File.AppendAllLines(this.IncludedFilePath, new[] { IncludedFileLastLine });
            File.WriteAllLines(this.DeepIncludedFilePath, deepIncludedFileLines);
            File.AppendAllLines(this.DeepIncludedFilePath, new[] { DeepIncludedFileLastLine });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void Build_From_Directory_Benchmark(int tasksCount)
        {
            var indexService = this.GetIndexService(tasksCount);
            var stopWatch = Stopwatch.StartNew();

            indexService.StartBuildIndex();

            while (indexService.Find(FirstFileLastLine).Count == 0 || indexService.Find(SecondFileLastLine).Count == 0 ||
                   indexService.Find(IncludedFileLastLine).Count == 0 || indexService.Find(DeepIncludedFileLastLine).Count == 0)
            {
                Thread.Sleep(100);

                if (stopWatch.Elapsed.TotalSeconds > 60)
                {
                    indexService.StopBuildIndex();
                    throw new Exception("To long execution");
                }
            }

            Console.WriteLine($"Elapsed for tasksCount {tasksCount}: {stopWatch.Elapsed}");
        }

        private IIndexService GetIndexService(int tasksCount)
        {
            return new IndexService(new InvertedIndex(new DefaultTokenizer()), new DirectoryObserver(this.PathToFiles, s => true), tasksCount);
        }
    }
}