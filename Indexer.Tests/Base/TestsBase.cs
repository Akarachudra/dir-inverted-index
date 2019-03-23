using System.IO;
using NUnit.Framework;

namespace Indexer.Tests.Base
{
    public class TestsBase
    {
        protected readonly string FirstFilePath;
        protected readonly string SecondFilePath;
        protected readonly string IncludedFilePath;
        protected readonly string IncludedDirPath;
        protected readonly string DeepIncludedDirPath;
        protected readonly string DeepIncludedFilePath;
        protected readonly string PathToFiles;
        private const string TestFilesFolder = "Test_Data";
        private const string FirstFileName = "FirstFileName.txt";
        private const string SecondFileName = "SecondFileName.txt";
        private const string IncludedFileName = "IncludedFileName.txt";
        private const string DeepIncludedFileName = "DeepIncludedFileName.txt";
        private const string IncludedDir = "IncludedDir";
        private const string DeepIncludedDir = "DeepIncludedDir";

        public TestsBase()
        {
            this.PathToFiles = GetPathToTemplates();
            this.FirstFilePath = Path.Combine(this.PathToFiles, FirstFileName);
            this.SecondFilePath = Path.Combine(this.PathToFiles, SecondFileName);
            this.IncludedDirPath = Path.Combine(this.PathToFiles, IncludedDir);
            this.DeepIncludedDirPath = Path.Combine(this.IncludedDirPath, DeepIncludedDir);
            this.IncludedFilePath = Path.Combine(this.IncludedDirPath, IncludedFileName);
            this.DeepIncludedFilePath = Path.Combine(this.DeepIncludedDirPath, DeepIncludedFileName);
        }

        [SetUp]
        public void RunBeforeTests()
        {
            EnsureErasedFolder(this.PathToFiles);
        }

        protected static string GetPathToTemplates()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, TestFilesFolder);
        }

        protected static void EnsureErasedFolder(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            Directory.CreateDirectory(path);
        }
    }
}