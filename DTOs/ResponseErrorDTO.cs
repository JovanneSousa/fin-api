namespace fin_api.DTOs
{
    public class ResponseErrorDTO
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public ResponseErrorDTO (bool success, IEnumerable<string> errors)
        {
            Success = success; 
            Errors = errors; 
        }
    }

}
