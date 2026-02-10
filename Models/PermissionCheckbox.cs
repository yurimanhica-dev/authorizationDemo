namespace AuthorizationDemo.Models;

public class PermissionCheckbox
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
