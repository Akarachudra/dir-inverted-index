using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer
{
    public interface IDirectorySearcher
    {
        IList<StoredResult> Find(string phrase);
    }
}