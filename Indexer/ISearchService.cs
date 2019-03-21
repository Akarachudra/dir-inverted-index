using System.Collections;
using System.Collections.Generic;

namespace Indexer
{
    public interface ISearchService
    {
        IList<SearchResult> FindString(string searchQuery);
    }
}