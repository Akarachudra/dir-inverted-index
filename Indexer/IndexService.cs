using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Indexer.Indexes;
using Indexer.Watch;

namespace Indexer
{
    public class IndexService : IIndexService
    {
        private readonly IInvertedIndex index;
        private readonly IDirectoryObserver directoryObserver;
        private readonly int buildTasksCount;
        private readonly ConcurrentQueue<FileSystemEventArgs> eventsQueue;
        private readonly TimeSpan consumeWaitDelay = TimeSpan.FromSeconds(1);
        private Task[] buildTasks;
        private CancellationTokenSource cts;

        public IndexService(IInvertedIndex index, IDirectoryObserver directoryObserver, int buildTasksCount = 2)
        {
            if (buildTasksCount <= 0)
            {
                throw new ArgumentException("Invalid index build tasks count");
            }

            this.index = index ?? throw new ArgumentException("Index should not be null");
            this.directoryObserver = directoryObserver ?? throw new ArgumentException("Directory observer should not be null");
            this.buildTasksCount = buildTasksCount;
            this.eventsQueue = new ConcurrentQueue<FileSystemEventArgs>();
            this.directoryObserver.Created += (sender, args) => this.eventsQueue.Enqueue(args);
            this.directoryObserver.Start();
        }

        public void StartBuildIndex()
        {
            if (this.cts != null && !this.cts.IsCancellationRequested)
            {
                throw new ArgumentException("Already started");
            }

            this.cts = new CancellationTokenSource();
            var ct = this.cts.Token;
            this.buildTasks = new Task[this.buildTasksCount];
            for (var i = 0; i < this.buildTasksCount; i++)
            {
                this.buildTasks[i] = Task.Factory.StartNew(
                    async () =>
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            this.Consume();
                            await Task.Delay(this.consumeWaitDelay, ct).ConfigureAwait(false);
                        }
                    },
                    ct,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        public void StopBuildIndex()
        {
            this.cts.Cancel();
            // TODO: dispose observer or continue events collecting?
        }

        public IList<DocumentPosition> Find(string query)
        {
            return this.index.Find(query);
        }

        private void Consume()
        {
            while (this.eventsQueue.TryDequeue(out var args))
            {
                if (args.ChangeType == WatcherChangeTypes.Created)
                {
                    try
                    {
                        var path = args.FullPath;
                        var rowNumber = 1;
                        foreach (var line in File.ReadLines(path))
                        {
                            this.index.Add(line, rowNumber, path);
                            rowNumber++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}