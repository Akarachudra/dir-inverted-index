using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
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
            const int buildTimes = 3;
            var stopWatch = new Stopwatch();
            var tokenizer = new DefaultTokenizer();
            var lines = TestDataGenerator.GetRandomLines(50000, 20000);

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

        [Test]
        public void Search_Result_Is_Same_For_InvertedIndex_And_SimpleSearching()
        {
            var tokenizer = new DefaultTokenizer();
            var ticks = Environment.TickCount;
            Console.WriteLine($"Test ticks: {ticks}");
            var lines = TestDataGenerator.GetRandomLines(ticks);
            var invertedIndex = new InvertedIndex(tokenizer);
            var phrase = TestDataGenerator.GetSearchPhrase(ticks);
            BuildIndex(invertedIndex, lines);

            var inmemoryResult = InmemorySimpleSearch.Find(lines, phrase);

            invertedIndex.Find(phrase)
                         .Select(x => new { x.RowNumber, x.ColNumber })
                         .Should()
                         .BeEquivalentTo(inmemoryResult.Select(x => new { x.RowNumber, x.ColNumber }));
        }

        [Test]
        public void InvertedIndex_Should_Be_Faster_Than_Simple_Searching()
        {
            const int phrasesCount = 50;
            var phrases = new string[phrasesCount];
            var tickCount = Environment.TickCount;
            Console.WriteLine($"TickCount: {tickCount}");
            for (var i = 0; i < phrasesCount; i++)
            {
                phrases[i] = TestDataGenerator.GetSearchPhrase(tickCount + i);
            }

            var tokenizer = new DefaultTokenizer();
            var stopWatch = new Stopwatch();
            var lines = TestDataGenerator.GetRandomLines(tickCount, 50000);
            var invertedIndex = new InvertedIndex(tokenizer);
            BuildIndex(invertedIndex, lines);

            stopWatch.Start();
            for (var i = 0; i < phrasesCount; i++)
            {
                var elapsedBefore = stopWatch.Elapsed;
                invertedIndex.Find(phrases[i]);
                var elapsed = stopWatch.Elapsed - elapsedBefore;
                Console.WriteLine($"Elapsed for phrase: {phrases[i]} {elapsed}");
            }

            var indexSearchingTime = stopWatch.Elapsed;

            stopWatch.Restart();
            for (var i = 0; i < phrasesCount; i++)
            {
                InmemorySimpleSearch.Find(lines, phrases[i]);
            }

            var simpleSearchingTime = stopWatch.Elapsed;

            Console.WriteLine($"InvertedIndex searching time: {indexSearchingTime}");
            Console.WriteLine($"Simple searching time: {simpleSearchingTime}");
            indexSearchingTime.Should().BeLessThan(simpleSearchingTime);
        }

        private static void BuildIndex(IInvertedIndex index, string[] lines)
        {
            index.Add(lines, "doc");
        }
    }
}