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
        public void Can_Get_All_Included_Directory_Files()
        {
            EnsureErasedFolder(this.IncludedDirPath);
            File.WriteAllText(this.FirstFilePath, "t");
            File.WriteAllText(this.IncludedFilePath, "t");

            var fileInfos = FileHelper.GetAllFiles(this.PathToFiles);

            fileInfos.Select(x => x.FullName).Should().BeEquivalentTo(this.FirstFilePath, this.IncludedFilePath);
        }

        [Test]
        public void Can_Determine_File_Is_Text()
        {
            const string s = "7310 dfsd \t 232 \n \r ывиьчбс !@#$%^&*()_+_ ";
            File.WriteAllText(this.FirstFilePath, s, Encoding.UTF8);

            FileHelper.IsTextFile(this.FirstFilePath).Should().BeTrue();

            EnsureErasedFolder(this.PathToFiles);

            File.WriteAllText(this.FirstFilePath, s, Encoding.ASCII);

            FileHelper.IsTextFile(this.FirstFilePath).Should().BeTrue();

            EnsureErasedFolder(this.PathToFiles);

            File.WriteAllText(this.FirstFilePath, s, Encoding.Unicode);

            FileHelper.IsTextFile(this.FirstFilePath).Should().BeTrue();
        }

        [TestCase(
            "89 50 4E 47 0D 0A 1A 0A 00 00 00 0D 9 48 44 52 00 00 01 00 00 00 01 00 01 03 00 00 00 66 BC 3A 25 00 00 00 03 50 4C 54 45 B5 D0 D0 63 04 16 EA 00 00 00 1F 49 44 41 54 68 81 ED C1 01 0D 00 00 00 C2 A0 F7 4F 6D 0E 37 A0 00 00 00 00 00 00 00 00 BE 0D 21 00 00 01 9A 60 E1 D5 00 00 00 00 49 45 4E 44 AE 42 60 82")]
        [TestCase(
            "FF D8 FF E0 00 10 4A 46 49 46 00 01 01 01 00 48 00 48 00 00 FF DB 00 43 00 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF C2 00 0B 08 00 01 00 01 01 01 11 00 FF C4 00 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF DA 00 08 01 01 00 01 3F 10")]
        public void Can_Determine_That_File_Is_Not_Text(string notTextFileHexByteString)
        {
            var bytes = notTextFileHexByteString.Split(' ')
                                                .Select(item => Convert.ToByte(item, 16))
                                                .ToArray();
            File.WriteAllBytes(this.FirstFilePath, bytes);

            FileHelper.IsTextFile(this.FirstFilePath).Should().BeFalse();
        }
    }
}