namespace CellStore.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Rol { get; set; } = "usuario";
}

public class RegistroDto
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class LoginDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}