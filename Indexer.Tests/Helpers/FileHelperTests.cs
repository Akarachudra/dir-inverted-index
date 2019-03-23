using System.IO;
using System.Linq;
using FluentAssertions;
using Indexer.Helpers;
using Indexer.Tests.Base;
using NUnit.Framework;

namespace Indexer.Tests.Helpers
{
    [TestFixture]
    public class FileHelperTests : TestsBase
    {
        [Test]
        public void Can_Get_All_Included_In_Directory_FileInfos()
        {
            EnsureErasedFolder(this.IncludedDirPath);
            File.WriteAllText(this.FirstFilePath, "t");
            File.WriteAllText(this.IncludedFilePath, "t");

            var fileInfos = FileHelper.GetAllFiles(this.PathToFiles);

            fileInfos.Select(x => x.FullName).Should().BeEquivalentTo(this.FirstFilePath, this.IncludedFilePath);
        }
    }
}