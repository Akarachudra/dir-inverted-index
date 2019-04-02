using System;
using FluentAssertions;
using Indexer.Watch;
using NUnit.Framework;

namespace Indexer.Tests.Watch
{
    [TestFixture]
    public class DirectoryObserverTests
    {
        [Test]
        public void Throw_Exception_If_Directory_Not_Exists()
        {
            var notExistsPath = @"F:\somepath\somepath\noexists";

            Action createAction = () => { new DirectoryObserver(notExistsPath, s => true); };

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Directory not exists");
        }
    }
}