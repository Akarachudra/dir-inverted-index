using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Indexer.Indexes;
using Indexer.Watch;

namespace Indexer
{
    public class IndexService : IIndexService
    {
        private readonly IInvertedIndex index;
        private readonly IDirectoryObserver directoryObserver;
        private readonly ConcurrentQueue<FileSystemEventArgs> eventsQueue;

        public IndexService(string path, IInvertedIndex index, IDirectoryObserver directoryObserver, int buildTasksCount = 2)
        {
            if (buildTasksCount <= 0)
            {
                throw new ArgumentException("Invalid index build tasks count");
            }

            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Path doesn't exists");
            }

            this.index = index ?? throw new ArgumentException("Index should not be null");
            this.directoryObserver = directoryObserver ?? throw new ArgumentException("Directory observer should not be null");
            this.directoryObserver = directoryObserver;
            this.eventsQueue = new ConcurrentQueue<FileSystemEventArgs>();
        }

        public void StartBuildIndex()
        {
            throw new System.NotImplementedException();
        }

        public void StopBuildIndex()
        {
            throw new System.NotImplementedException();
        }

        public IList<StoredResult> Find(string phrase)
        {
            throw new System.NotImplementedException();
        }

        private void ConsumeAction()
        {
        }
    }
}