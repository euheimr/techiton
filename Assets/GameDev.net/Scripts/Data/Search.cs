/// <summary>
/// Data holder for a search.
/// </summary>
namespace gamedev.net.data
{
    public class Search
    {
        public int page;
        public int perPage;
        public int totalResults;
        public int totalPages;
        public Result[] results;

        public Search()
        {

        }
    }
}