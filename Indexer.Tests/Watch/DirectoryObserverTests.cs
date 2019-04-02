using System;
using System.IO;
using FluentAssertions;
using Indexer.Tests.Base;
using Indexer.Watch;
using NUnit.Framework;

namespace Indexer.Tests.Watch
{
    [TestFixture]
    public class DirectoryObserverTests : TestsBase
    {
        [Test]
        public void Throw_Exception_If_Directory_Not_Exists()
        {
            var notExistsPath = @"F:\somepath\somepath\noexists";

            Action createAction = () => { new DirectoryObserver(notExistsPath, s => true); };

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Directory not exists");
        }

        [Test]
        public void Notify_Created_Event_For_Exists_Files()
        {
            var changeType = WatcherChangeTypes.All;
            var callBackedName = string.Empty;
            var callBackedFullPath = string.Empty;
            var callCount = 0;
            File.WriteAllText(this.FirstFilePath, "text");
            var observer = new DirectoryObserver(this.PathToFiles, s => true);
            FileSystemEventHandler callBackHandler = (sender, args) =>
            {
                callBackedFullPath = args.FullPath;
                callBackedName = args.Name;
                changeType = args.ChangeType;
                callCount++;
            };

            observer.Created += callBackHandler;
            observer.Start();

            callCount.Should().Be(1);
            callBackedName.Should().Be(FirstFileName);
            changeType.Should().Be(WatcherChangeTypes.Created);
            callBackedFullPath.Should().Be(this.FirstFilePath);
        }

        [Test]
        public void Do_Not_Notify_Created_Event_For_Unsuitable_Files()
        {
            var callCount = 0;
            File.WriteAllText(this.FirstFilePath, "text");
            var observer = new DirectoryObserver(this.PathToFiles, s => false);
            FileSystemEventHandler callBackHandler = (sender, args) => { callCount++; };

            observer.Created += callBackHandler;
            observer.Start();

            callCount.Should().Be(0);
        }
    }
}