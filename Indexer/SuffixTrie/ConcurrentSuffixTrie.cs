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

        public void Insert(string s, StoredResult storedResult)
        {
            var curResult = storedResult;
            for (var i = 0; i < s.Length; i++)
            {
                var curNode = this.root;
                curResult = new StoredResult
                {
                    ColNumber = curResult.ColNumber + i,
                    PathHash = curResult.PathHash,
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

        public IList<StoredResult> Find(string suffix)
        {
            var curNode = this.root;
            foreach (var c in suffix)
            {
                if (!curNode.Childrens.TryGetValue(c, out curNode))
                {
                    return new List<StoredResult>();
                }
            }

            return curNode.Matches;
        }
    }
}