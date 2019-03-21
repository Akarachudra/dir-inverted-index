using System.Collections.Generic;
using System.IO;

namespace Indexer
{
    public class DummySearcher : ISearchService
    {
        private readonly string filesPath;

        public DummySearcher(string filesPath)
        {
            this.filesPath = filesPath;
        }

        public IList<SearchResult> FindString(string searchQuery)
        {
            throw new System.NotImplementedException();
        }
    }
}