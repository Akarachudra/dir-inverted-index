namespace Indexer
{
    public interface IIndexService : ISearcher
    {
        void StartBuildIndex();

        void StopBuildIndex();
    }
}