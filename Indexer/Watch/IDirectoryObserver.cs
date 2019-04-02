using System;
using System.IO;

namespace Indexer.Watch
{
    public interface IDirectoryObserver : IDisposable
    {
        event FileSystemEventHandler Changed;

        event FileSystemEventHandler Created;

        event FileSystemEventHandler Deleted;

        event RenamedEventHandler Renamed;

        void Start();
    }
}