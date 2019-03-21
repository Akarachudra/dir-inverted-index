using System.Collections.Generic;

namespace Indexer
{
    public class IndexedSearcher : ISearchService
    {
        private readonly string filesPath;

        public IndexedSearcher(string filesPath)
        {
            this.filesPath = filesPath;
        }

        public IList<SearchResult> FindString(string searchQuery)
        {
            throw new System.NotImplementedException();
        }
    }
}