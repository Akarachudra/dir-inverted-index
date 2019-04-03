using System;
using System.Collections.Generic;

namespace Indexer.Helpers
{
    public static class StringHelper
    {
        public static int GetHashCode(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            unchecked
            {
                var hash1 = (5381 << 16) + 5381;
                var hash2 = hash1;

                for (var i = 0; i < s.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ s[i];
                    if (i == s.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ s[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }

        public static IEnumerable<int> AllIndexesOf(string s, string substring)
        {
            if (string.IsNullOrEmpty(substring))
            {
                yield break;
            }

            var minIndex = s.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = s.IndexOf(substring, minIndex + substring.Length, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}