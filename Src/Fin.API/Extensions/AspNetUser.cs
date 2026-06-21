using System.Security.Claims;

namespace Fin.Api.Extensions
{
    public class AspNetUser : IUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Name => _accessor.HttpContext.User.Identity.Name;

        public IEnumerable<Claim> GetClaimsIdentity() =>
            _accessor.HttpContext.User.Claims;

        public string GetUserEmail() =>
            IsAuthenticated() ? _accessor.HttpContext.User.GetUserEmail() : "";

        public string GetUserId() =>
            IsAuthenticated() ? _accessor.HttpContext.User.GetUserId() : string.Empty;

        public bool IsAuthenticated() =>
            _accessor.HttpContext.User.Identity.IsAuthenticated;

        public bool IsInRole(string role) =>
            _accessor.HttpContext.User.IsInRole(role);
    }
}
