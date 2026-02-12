using System.ComponentModel.DataAnnotations;

namespace AuthorizationDemo.Models;

public class UserViewModel
{
    public string? Id { get; set; }
    [Required(ErrorMessage = "O campo Nome Completo é obrigatório!")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "O campo Email é obrigatório!")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string? Password { get; set; }
    public List<RoleItem> Roles { get; set; } = new List<RoleItem>();
}
