using CellStore.Data;
using CellStore.Filters;
using CellStore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace CellStore.Controllers;

[ApiController]
[Route("api/estadisticas")]
[RequiereAdmin]
public class EstadisticasController : ControllerBase
{
    private readonly Db _db;
    public EstadisticasController(Db db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        using var con = _db.Conectar();

        var resumen = await con.QueryFirstAsync<Resumen>(@"
            select
                (select isnull(sum(total), 0) from pedidos) as Ventas,
                (select count(*) from pedidos) as Pedidos,
                (select count(*) from usuarios) as Usuarios,
                (select count(*) from celulares where stock <= 3) as StockBajo");

        var ventas = await con.QueryAsync<VentaDia>(@"
            select convert(varchar(10), fecha, 23) as Dia,
                   sum(total) as Total,
                   count(*) as Pedidos
            from pedidos
            where fecha >= dateadd(day, -29, convert(date, getdate()))
            group by convert(varchar(10), fecha, 23)
            order by Dia");

        var vendidos = await con.QueryAsync<MasVendido>(@"
            select top 5 c.marca + ' ' + c.modelo as Nombre,
                   sum(d.cantidad) as Unidades
            from detalle_pedido d
            join celulares c on c.id = d.celular_id
            group by c.marca, c.modelo
            order by Unidades desc");

        var bajo = await con.QueryAsync<StockBajoItem>(@"
            select marca + ' ' + modelo as Nombre, stock as Stock
            from celulares
            where stock <= 3
            order by stock");

        return Ok(new
        {
            resumen,
            ventasPorDia = ventas,
            masVendidos = vendidos,
            stockBajo = bajo
        });
    }
}