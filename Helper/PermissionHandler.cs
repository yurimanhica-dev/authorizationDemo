using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Verifica se o usuário tem o claim de permissão
        if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.PermissionName))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
