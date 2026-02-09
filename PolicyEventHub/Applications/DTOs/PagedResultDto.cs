namespace PolicyEventHub.Applications.DTOs
{
    public class PagedResultDto<T>
    {
        public required IEnumerable<T> Items { get; init; }
        public required int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
