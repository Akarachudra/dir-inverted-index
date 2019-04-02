using System;
using System.Collections.Concurrent;
using System.IO;
using Indexer.Helpers;

namespace Indexer.Watch
{
    // TODO: impl deleted, renamed events
    public class DirectoryObserver : IDirectoryObserver
    {
        private readonly string path;
        private readonly Func<string, bool> isSuitableFile;
        private readonly FileSystemWatcher watcher;
        private readonly ConcurrentDictionary<string, DateTime> lastChangesDictionary;

        public DirectoryObserver(string path, Func<string, bool> isSuitableFile)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory not exists");
            }

            this.lastChangesDictionary = new ConcurrentDictionary<string, DateTime>();
            this.path = path;
            this.isSuitableFile = isSuitableFile;
            this.watcher = new FileSystemWatcher(path);
            this.watcher.Changed += this.WatcherOnChanged;
            this.watcher.Created += this.WatcherOnCreated;
        }

        public event FileSystemEventHandler Changed;

        public event FileSystemEventHandler Created;

        public event FileSystemEventHandler Deleted;

        public event RenamedEventHandler Renamed;

        public void Start()
        {
            this.watcher.IncludeSubdirectories = true;
            this.watcher.EnableRaisingEvents = true;
            var fileInfos = FileHelper.GetAllFiles(this.path);
            foreach (var fileInfo in fileInfos)
            {
                this.WatcherOnCreated(this.watcher, new FileSystemEventArgs(WatcherChangeTypes.Created, fileInfo.DirectoryName, fileInfo.Name));
            }
        }

        public void Dispose()
        {
            this.watcher.Dispose();
        }

        protected virtual void OnChanged(FileSystemEventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }

        protected virtual void OnCreated(FileSystemEventArgs e)
        {
            this.Created?.Invoke(this, e);
        }

        protected virtual void OnDeleted(FileSystemEventArgs e)
        {
            this.Deleted?.Invoke(this, e);
        }

        protected virtual void OnRenamed(RenamedEventArgs e)
        {
            this.Renamed?.Invoke(this, e);
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.isSuitableFile(e.FullPath))
            {
                return;
            }

            if (this.lastChangesDictionary.ContainsKey(e.FullPath) && this.lastChangesDictionary[e.FullPath] <= DateTime.UtcNow.AddSeconds(1))
            {
                return;
            }

            this.lastChangesDictionary[e.FullPath] = DateTime.UtcNow;
            this.OnChanged(e);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                return;
            }

            if (!this.isSuitableFile(e.FullPath))
            {
                return;
            }

            this.OnCreated(e);
        }
    }
}