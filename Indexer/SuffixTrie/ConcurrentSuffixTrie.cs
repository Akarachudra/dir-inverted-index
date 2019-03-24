using System.Collections.Generic;

namespace Indexer.SuffixTrie
{
    public class ConcurrentSuffixTrie
    {
        private readonly TrieNode root;

        public ConcurrentSuffixTrie()
        {
            this.root = new TrieNode();
        }

        public void Insert(string s, SearchResult searchResult)
        {
            var curResult = searchResult;
            for (var i = 0; i < s.Length; i++)
            {
                var curNode = this.root;
                curResult = new SearchResult
                {
                    ColNumber = curResult.ColNumber + i,
                    FilePath = curResult.FilePath,
                    RowNumber = curResult.RowNumber
                };
                for (var j = i; j < s.Length; j++)
                {
                    var c = s[j];
                    if (!curNode.Childrens.ContainsKey(c))
                    {
                        curNode.Childrens.TryAdd(c, new TrieNode());
                    }

                    curNode = curNode.Childrens[c];
                    curNode.Matches.Add(curResult);
                }
            }
        }

        public IList<SearchResult> Find(string suffix)
        {
            var curNode = this.root;
            foreach (var c in suffix)
            {
                if (!curNode.Childrens.TryGetValue(c, out curNode))
                {
                    return new List<SearchResult>();
                }
            }

            return curNode.Matches;
        }
    }
}