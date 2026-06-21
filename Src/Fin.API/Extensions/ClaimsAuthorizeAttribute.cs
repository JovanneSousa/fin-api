using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fin.Api.Extensions
{
    public class ClaimsAuthorizeAttribute : TypeFilterAttribute
    {
        public ClaimsAuthorizeAttribute(
            string claimName,
            string claimValue
            ) : base (typeof(RequisitoClaimFilter))
        {
            Arguments = new object[] { new Claim(claimName, claimValue) };
        }
    }
}
