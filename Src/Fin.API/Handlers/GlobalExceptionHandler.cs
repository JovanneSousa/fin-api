using Fin.Application.http.ResponseDTO;
using Microsoft.AspNetCore.Diagnostics;

namespace Fin.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            var (status, titulo) = exception switch
            {
                ArgumentException => (400, "Requisição inválida!"),
                _ => (500, "Erro Interno do servidor")
            };

            if (status == 500)
                _logger.LogError(exception, "Erro inesperado:{Mensagem}", exception.Message);
            else
                _logger.LogWarning("Exceção tratada [{status}]:{Mensagem}",status , exception.Message);

            var problem = new ApiResponse
            {
                Success = false,
                Errors = new List<string> { titulo }
            };

            httpContext.Response.StatusCode = status;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response
                .WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
