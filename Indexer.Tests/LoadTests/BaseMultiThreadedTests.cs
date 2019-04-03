using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Indexer.Indexes;
using Indexer.Tests.Base;
using NUnit.Framework;

namespace Indexer.Tests.LoadTests
{
    [TestFixture]
    public abstract class BaseMultiThreadedTests : TestsBase
    {
        [Test]
        public void Add_And_Read_Simultaneously()
        {
            var exceptionsCount = 0;
            var index = this.GetNewIndex();
            var ticks = 100;
            Action addAction = () =>
            {
                try
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var lines = TestDataGenerator.GetRandomLines(ticks + i, 10000);
                        index.Add(lines, "some doc" + i);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Interlocked.Increment(ref exceptionsCount);
                    throw;
                }
            };
            var stopWatch = Stopwatch.StartNew();
            var addTasks = new Task[2];
            for (var i = 0; i < addTasks.Length; i++)
            {
                addTasks[i] = new Task(addAction);
                addTasks[i].Start();
            }

            Action readAction = () =>
            {
                try
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        var phrase = TestDataGenerator.GetSearchPhrase(ticks + i);
                        index.Find(phrase);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Interlocked.Increment(ref exceptionsCount);
                    throw;
                }
            };
            var readTasks = new Task[2];
            for (var i = 0; i < readTasks.Length; i++)
            {
                readTasks[i] = new Task(readAction);
                readTasks[i].Start();
            }

            Task.WaitAll(addTasks);
            Console.WriteLine($"Build elapsed: {stopWatch.Elapsed}");
            Task.WaitAll(readTasks);
            Console.WriteLine($"Total elapsed: {stopWatch.Elapsed}");

            exceptionsCount.Should().Be(0);
        }

        protected abstract IInvertedIndex GetNewIndex();
    }
}