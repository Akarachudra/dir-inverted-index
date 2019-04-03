using System.Collections.Generic;
using System.IO;
using Indexer.Helpers;
using Indexer.Indexes;

namespace Indexer
{
    public class SimpleDirectorySearcher : ISearcher
    {
        private readonly string filesPath;

        public SimpleDirectorySearcher(string filesPath)
        {
            this.filesPath = filesPath;
        }

        public IList<DocumentPosition> Find(string phrase)
        {
            var result = new List<DocumentPosition>();
            if (string.IsNullOrEmpty(phrase))
            {
                return result;
            }

            phrase = phrase.Trim();
            var files = FileHelper.GetAllFiles(this.filesPath);
            foreach (var file in files)
            {
                var rowNumber = 1;

                foreach (var line in File.ReadLines(file.FullName))
                {
                    foreach (var index in StringHelper.AllIndexesOf(line, phrase))
                    {
                        result.Add(
                            new DocumentPosition
                            {
                                Document = file.FullName,
                                ColNumber = index + 1,
                                RowNumber = rowNumber
                            });
                    }

                    rowNumber++;
                }
            }

            return result;
        }
    }
}