using System.Collections.Generic;

namespace Indexer
{
    public class InvertedIndex
    {
        private readonly SortedList<string, IList<SearchResult>> list;

        public InvertedIndex()
        {
            this.list = new SortedList<string, IList<SearchResult>>();
        }

        public void Add(string word, SearchResult curResult)
        {
            var length = word.Length;
            for (var i = 0; i < word.Length; i++)
            {
                curResult = new SearchResult
                {
                    ColNumber = curResult.ColNumber + i,
                    FilePath = curResult.FilePath,
                    RowNumber = curResult.RowNumber
                };

                var suffix = word.Substring(i, length - i);
                if (!this.list.ContainsKey(suffix))
                {
                    this.list.Add(suffix, new List<SearchResult>());
                }

                this.list[suffix].Add(curResult);
            }
        }

        public IList<SearchResult> Find(string query)
        {
            if (this.list.ContainsKey(query))
            {
                return this.list[query];
            }

            return new List<SearchResult>();
        }
    }
}