namespace fin_api.DTOs
{
    public class ResponseSuccessDTO<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }

        public ResponseSuccessDTO(bool success, T data)
        {
            Success = success;
            Data = data;
        }
    }
}
