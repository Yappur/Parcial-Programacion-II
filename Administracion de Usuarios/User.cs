namespace GestionVentas;

internal sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Vendedor";
    public bool IsActive { get; set; }
}

internal static class UserPermissions
{
    public static bool CanManageUsers(User user) => user.Role == "Administrador";
}
