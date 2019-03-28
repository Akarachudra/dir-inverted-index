using System.Collections.Generic;

namespace Indexer.Indexes
{
    public interface IInvertedIndex
    {
        void Add(string line, int rowNumber, string pathHash);

        IList<StoredResult> Find(string query);
    }
}