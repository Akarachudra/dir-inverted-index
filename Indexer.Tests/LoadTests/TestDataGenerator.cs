using System;
using System.Text;

namespace Indexer.Tests.LoadTests
{
    public static class TestDataGenerator
    {
        public static string[] Terms = new[]
        {
            "program", " ", "var", "const", ".", "class", "interface", "Method()", "int", "string", "static", "  ", ";", "[]", "IList<>", "Dictionary",
            "Concurrent", "System", "using", "(", ")", "i", "++", "namespace", "{", "}", "true", "false", "for", "ToString()", "char", "=", ">", "<", "==",
            "return", "Get()"
        };

        public static string[] GetRandomLines(int seed)
        {
            const int linesCount = 5000;
            var random = new Random(seed);
            var lines = new string[linesCount];
            var strBuilder = new StringBuilder();
            for (var i = 0; i < linesCount; i++)
            {
                var termsCount = random.Next(3, 10);
                for (var j = 0; j < termsCount; j++)
                {
                    var term = Terms[random.Next(Terms.Length)];
                    strBuilder.Append(term);
                }

                lines[i] = strBuilder.ToString();
                strBuilder.Clear();
            }

            return lines;
        }

        public static string GetSearchPhrase(int seed)
        {
            var random = new Random(seed);
            var termsCount = random.Next(1, 3);
            var strBuilder = new StringBuilder();
            for (var j = 0; j < termsCount; j++)
            {
                var term = Terms[random.Next(Terms.Length)];
                strBuilder.Append(term);
            }

            return strBuilder.ToString();
        }
    }
}