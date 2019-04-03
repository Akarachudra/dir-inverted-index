using System.IO;
using NUnit.Framework;

namespace Indexer.Tests.Base
{
    public class TestsBase
    {
        protected const string TestFilesFolder = "Test_Files";
        protected const string FirstFileName = "FirstFileName.txt";
        protected const string SecondFileName = "SecondFileName.txt";
        protected const string IncludedFileName = "IncludedFileName.txt";
        protected const string DeepIncludedFileName = "DeepIncludedFileName.txt";
        protected const string IncludedDir = "IncludedDir";
        protected const string DeepIncludedDir = "DeepIncludedDir";
        protected readonly string FirstFilePath;
        protected readonly string SecondFilePath;
        protected readonly string IncludedFilePath;
        protected readonly string IncludedDirPath;
        protected readonly string DeepIncludedDirPath;
        protected readonly string DeepIncludedFilePath;
        protected readonly string PathToFiles;

        public TestsBase()
        {
            this.PathToFiles = GetPathToTestFiles();
            this.FirstFilePath = Path.Combine(this.PathToFiles, FirstFileName);
            this.SecondFilePath = Path.Combine(this.PathToFiles, SecondFileName);
            this.IncludedDirPath = Path.Combine(this.PathToFiles, IncludedDir);
            this.DeepIncludedDirPath = Path.Combine(this.IncludedDirPath, DeepIncludedDir);
            this.IncludedFilePath = Path.Combine(this.IncludedDirPath, IncludedFileName);
            this.DeepIncludedFilePath = Path.Combine(this.DeepIncludedDirPath, DeepIncludedFileName);
        }

        [SetUp]
        public virtual void RunBeforeTests()
        {
            EnsureErasedFolder(this.PathToFiles);
        }

        protected static string GetPathToTestFiles()
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