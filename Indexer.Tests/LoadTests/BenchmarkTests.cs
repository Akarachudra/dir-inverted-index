using System;
using System.Collections.Generic;
using System.Diagnostics;
using Indexer.Indexes;
using Indexer.Tests.Base;
using Indexer.Tokens;
using NUnit.Framework;

namespace Indexer.Tests.LoadTests
{
    [TestFixture]
    public class BenchmarkTests : TestsBase
    {
        [Test]
        public void Indexes_Build_Time()
        {
            const int buildTimes = 5;
            var stopWatch = new Stopwatch();
            var tokenizer = new DefaultTokenizer();
            var lines = TestDataGenerator.GetRandomLines(50000);

            stopWatch.Start();
            for (var i = 0; i < buildTimes; i++)
            {
                BuildIndex(new InvertedHashIndex(tokenizer), lines);
            }

            var hashIndexBuildTime = stopWatch.Elapsed;

            stopWatch.Restart();
            for (var i = 0; i < buildTimes; i++)
            {
                BuildIndex(new InvertedIndex(tokenizer), lines);
            }

            var suffixIndeBuildTime = stopWatch.Elapsed;

            Console.WriteLine($"HashIndex build time: {hashIndexBuildTime}");
            Console.WriteLine($"SuffixIndex build time: {suffixIndeBuildTime}");
        }

        private static void BuildIndex(IInvertedIndex index, IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                index.Add(line, 1, null);
            }
        }
    }
}