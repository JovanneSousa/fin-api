using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace fin_api.Extensions
{
    public class RequisitoClaimFilter : IAuthorizationFilter
    {
        private readonly Claim _claim;

        public RequisitoClaimFilter(Claim claim)
        {
            _claim = claim;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {

            if (!context.HttpContext.User.Identity.IsAuthenticated) 
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    errors = new[] { "Usuário não autenticado!" }
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if(!CustomAuthorization.ValidarClaimUsuario(context.HttpContext, _claim.Type, _claim.Value))
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    errors = new[] { "Você não tem permissão para acessar este recurso!" }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
