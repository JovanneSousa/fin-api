using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fin.Api.Configuration.Swagger
{
    public class AuthResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            if (authAttributes.Any())
            {
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Usuário não autenticado" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Usuário não tem permissão para acessar este recurso" });
            }
        }
    }
}
