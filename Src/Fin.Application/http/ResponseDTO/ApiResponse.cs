namespace Fin.Application.http.ResponseDTO
{
    public record ApiResponse
    {
        public bool Success {  get; set; }
        public List<String>? Errors {  get; set; }
    }

    public record ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}
