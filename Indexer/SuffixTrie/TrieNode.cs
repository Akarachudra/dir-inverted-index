using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Indexer.SuffixTrie
{
    public class TrieNode
    {
        private ConcurrentDictionary<char, TrieNode> childrens;
        private IList<StoredResult> matches;

        public ConcurrentDictionary<char, TrieNode> Childrens
        {
            get => this.childrens ?? (this.childrens = new ConcurrentDictionary<char, TrieNode>());
            set => this.childrens = value;
        }

        public IList<StoredResult> Matches
        {
            get => this.matches ?? (this.matches = new List<StoredResult>());
            set => this.matches = value;
        }
    }
}