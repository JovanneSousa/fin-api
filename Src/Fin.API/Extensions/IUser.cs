using System.Security.Claims;

namespace Fin.Api.Extensions
{
    public interface IUser
    {
        string Name { get; }
        string GetUserId();
        string GetUserEmail();
        bool IsAuthenticated();
        bool IsInRole(string role);
        IEnumerable<Claim> GetClaimsIdentity();
    }
}
