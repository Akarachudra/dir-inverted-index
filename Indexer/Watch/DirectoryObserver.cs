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
        private readonly Func<string, bool> filterByPath;
        private readonly FileSystemWatcher watcher;
        private readonly ConcurrentDictionary<string, DateTime> lastChangesDictionary;

        public DirectoryObserver(string path, Func<string, bool> filterByPath)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory not exists");
            }

            this.lastChangesDictionary = new ConcurrentDictionary<string, DateTime>();
            this.path = path;
            this.filterByPath = filterByPath;
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
            var fullPath = e.FullPath;
            if (this.lastChangesDictionary.ContainsKey(fullPath) && this.lastChangesDictionary[fullPath] <= DateTime.UtcNow.AddSeconds(1))
            {
                return;
            }

            if (!this.IsGoodEvent(fullPath))
            {
                return;
            }

            this.lastChangesDictionary[fullPath] = DateTime.UtcNow;

            this.OnChanged(e);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (!this.IsGoodEvent(e.FullPath))
            {
                return;
            }

            this.OnCreated(e);
        }

        private bool IsGoodEvent(string fullPath)
        {
            if (Directory.Exists(fullPath))
            {
                return false;
            }

            if (!this.filterByPath(fullPath))
            {
                return false;
            }

            return true;
        }
    }
}