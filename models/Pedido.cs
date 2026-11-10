namespace CellStore.Models;

public class ItemDto
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
}

public class PedidoDto
{
    public List<ItemDto>? Items { get; set; }
}

public class PedidoResumen
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
}

public class PedidoCabecera
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
}

public class DetalleTicket
{
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}