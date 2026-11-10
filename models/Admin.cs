namespace CellStore.Models;

public class UsuarioLista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
}

public class RolDto
{
    public string? Rol { get; set; }
}

public class Resumen
{
    public decimal Ventas { get; set; }
    public int Pedidos { get; set; }
    public int Usuarios { get; set; }
    public int StockBajo { get; set; }
}

public class VentaDia
{
    public string Dia { get; set; } = "";
    public decimal Total { get; set; }
    public int Pedidos { get; set; }
}

public class MasVendido
{
    public string Nombre { get; set; } = "";
    public int Unidades { get; set; }
}

public class StockBajoItem
{
    public string Nombre { get; set; } = "";
    public int Stock { get; set; }
}
