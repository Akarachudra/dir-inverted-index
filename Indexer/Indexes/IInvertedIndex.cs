using System.Collections.Generic;

namespace Indexer.Indexes
{
    public interface IInvertedIndex
    {
        void Add(string line, int rowNumber, string document);

        IList<DocumentPosition> Find(string query);
    }
}