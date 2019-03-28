using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer
{
    public class IndexedSearcher : IDirectorySearcher
    {
        private readonly string filesPath;
        private readonly IInvertedIndex index;

        public IndexedSearcher(string filesPath, IInvertedIndex index)
        {
            this.filesPath = filesPath;
            this.index = index;
        }

        public IList<StoredResult> Find(string phrase)
        {
            throw new System.NotImplementedException();
        }
    }
}