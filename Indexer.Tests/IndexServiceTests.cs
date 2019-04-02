using System;
using System.Collections.Generic;
using FluentAssertions;
using Indexer.Indexes;
using Indexer.Tests.Base;
using Indexer.Tokens;
using Indexer.Watch;
using Moq;
using NUnit.Framework;

namespace Indexer.Tests
{
    [TestFixture]
    public class IndexServiceTests : TestsBase
    {
        private readonly Mock<IDirectoryObserver> observerMock = new Mock<IDirectoryObserver>();

        [TestCase(0)]
        [TestCase(-1)]
        public void Throw_Exception_If_Build_Tasks_Count_Is_Zero_Or_Lower(int tasksCount)
        {
            Action createAction = () => new IndexService(this.PathToFiles, new InvertedIndex(new DefaultTokenizer()), this.observerMock.Object, tasksCount);

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Invalid index build tasks count");
        }

        [Test]
        public void Throw_Exception_If_Passed_Path_Does_Not_Exists()
        {
            Action createAction = () => new IndexService("not exists path", new InvertedIndex(new DefaultTokenizer()), this.observerMock.Object);

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Path doesn't exists");
        }

        [Test]
        public void Throw_Exception_If_Passed_Index_Is_Null()
        {
            Action createAction = () => new IndexService(this.PathToFiles, null, this.observerMock.Object);

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Index should not be null");
        }

        [Test]
        public void Throw_Exception_If_Passed_DirectoryObserver_Is_Null()
        {
            Action createAction = () => new IndexService(this.PathToFiles, new InvertedIndex(new DefaultTokenizer()), null);

            createAction.Should().Throw<ArgumentException>().And.Message.Should().Be("Directory observer should not be null");
        }

        [Test]
        public void Find_Returns_Result_From_Index()
        {
            var indexMock = new Mock<IInvertedIndex>();
            var expectedResult = new List<DocumentPosition> { new DocumentPosition { RowNumber = 5 } };
            indexMock.Setup(x => x.Find("query phrase")).Returns(expectedResult);
            var indexService = new IndexService(this.PathToFiles, indexMock.Object, this.observerMock.Object);

            var result = indexService.Find("query phrase");

            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}