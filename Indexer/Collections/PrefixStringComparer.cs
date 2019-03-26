using System;
using System.Collections.Generic;

namespace Indexer.Collections
{
    public class PrefixStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Passed string could not be null");
            }

            if (y.Length > x.Length)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }

            return x.StartsWith(y) ? 0 : string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}