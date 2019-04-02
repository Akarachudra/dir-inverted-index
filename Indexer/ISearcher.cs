using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer
{
    public interface ISearcher
    {
        IList<DocumentPosition> Find(string phrase);
    }
}