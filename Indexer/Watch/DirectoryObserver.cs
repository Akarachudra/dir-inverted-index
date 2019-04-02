using System;
using System.IO;
using Indexer.Helpers;

namespace Indexer.Watch
{
    public class DirectoryObserver : IDirectoryObserver
    {
        private readonly string path;
        private readonly Func<string, bool> isSuitableFile;
        private readonly FileSystemWatcher watcher;

        public DirectoryObserver(string path, Func<string, bool> isSuitableFile)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory not exists");
            }

            this.path = path;
            this.isSuitableFile = isSuitableFile;
            this.watcher = new FileSystemWatcher(path);
            this.watcher.Changed += this.WatcherOnChanged;
        }

        public event FileSystemEventHandler Changed;

        public event FileSystemEventHandler Created;

        public event FileSystemEventHandler Deleted;

        public event RenamedEventHandler Renamed;

        public void Start()
        {
            var fileInfos = FileHelper.GetAllFiles(this.path);
            foreach (var fileInfo in fileInfos)
            {
                if (!this.isSuitableFile(fileInfo.FullName))
                {
                    continue;
                }

                this.OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, fileInfo.DirectoryName, fileInfo.Name));
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
            this.OnChanged(e);
        }
    }
}