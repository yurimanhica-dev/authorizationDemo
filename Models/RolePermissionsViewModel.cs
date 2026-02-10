using AuthorizationDemo.Models;

public class RolePermissionsViewModel
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public List<PermissionCheckbox> Permissions { get; set; }
}
