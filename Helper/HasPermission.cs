using System.Security.Claims;

namespace AuthorizationDemo.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            if (user == null) return false;
            return user.HasClaim(c => c.Type == "Permission" && c.Value == permission);
        }
    }
}
