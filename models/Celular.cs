namespace CellStore.Models;

public class Celular
{
    public int Id { get; set; }
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}