using System.Collections.Generic;

namespace Indexer.Indexes
{
    public interface IInvertedIndex
    {
        void Add(string[] lines, string document);

        IList<DocumentPosition> Find(string phrase);
    }
}