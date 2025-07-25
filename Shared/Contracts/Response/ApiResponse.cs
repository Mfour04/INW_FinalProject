namespace Shared.Contracts.Response
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; } = new object();

        public int? TotalPage { get; set; }
        public int? TotalResult { get; set; }
    }
}
