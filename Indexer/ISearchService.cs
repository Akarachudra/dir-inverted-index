using System.Collections;
using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer
{
    public interface ISearchService
    {
        IList<StoredResult> Find(string term);
    }
}