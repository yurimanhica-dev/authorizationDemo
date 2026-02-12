namespace AuthorizationDemo.Models;

public class RoleViewModel
{
    public string Id { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionCheckbox> Permissions { get; set; } = new();
}
