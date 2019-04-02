using System.Collections.Generic;
using Indexer.Indexes;

namespace Indexer
{
    public interface IIndexService
    {
        void StartBuildIndex();

        void StopBuildIndex();

        IList<StoredResult> Find(string phrase);
    }
}