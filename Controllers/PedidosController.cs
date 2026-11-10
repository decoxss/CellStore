using CellStore.Data;
using CellStore.Filters;
using CellStore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace CellStore.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly Db _db;
    public PedidosController(Db db) => _db = db;

    [HttpPost, RequiereLogin]
    public async Task<IActionResult> Crear(PedidoDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { error = "el carrito esta vacio" });

        if (dto.Items.Any(i => i.Id <= 0 || i.Cantidad <= 0))
            return BadRequest(new { error = "datos del carrito invalidos" });

        var usuarioId = HttpContext.Session.GetInt32("id")!.Value;

        using var con = _db.Conectar();
        await con.OpenAsync();
        using var tx = con.BeginTransaction();   // si no se hace Commit, se deshace todo

        var pedidoId = await con.ExecuteScalarAsync<int>(
            "insert into pedidos (usuario_id) output inserted.id values (@usuarioId)",
            new { usuarioId }, tx);

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            // descuenta stock solo si alcanza
            var filas = await con.ExecuteAsync(
                @"update celulares set stock = stock - @cant
                  where id = @id and stock >= @cant",
                new { id = item.Id, cant = item.Cantidad }, tx);

            if (filas == 0)
                return Conflict(new { error = "Un producto ya no tiene existencias suficientes o no existe." });

            // el precio sale de la base de datos, no del navegador
            var precio = await con.ExecuteScalarAsync<decimal>(
                "select precio from celulares where id = @id",
                new { id = item.Id }, tx);

            await con.ExecuteAsync(
                @"insert into detalle_pedido (pedido_id, celular_id, cantidad, precio_unitario)
                  values (@pedidoId, @celularId, @cant, @precio)",
                new { pedidoId, celularId = item.Id, cant = item.Cantidad, precio }, tx);

            total += precio * item.Cantidad;
        }

        await con.ExecuteAsync(
            "update pedidos set total = @total where id = @pedidoId",
            new { total, pedidoId }, tx);

        tx.Commit();
        return Created("", new { id = pedidoId, total });
    }

    [HttpGet("mios"), RequiereLogin]
    public async Task<IActionResult> Mios()
    {
        var usuarioId = HttpContext.Session.GetInt32("id")!.Value;

        using var con = _db.Conectar();
        var lista = await con.QueryAsync<PedidoResumen>(
            "select id, fecha, total from pedidos where usuario_id = @usuarioId order by id desc",
            new { usuarioId });
        return Ok(lista);
    }

    [HttpGet("{id:int}"), RequiereLogin]
    public async Task<IActionResult> Obtener(int id)
    {
        using var con = _db.Conectar();

        var p = await con.QueryFirstOrDefaultAsync<PedidoCabecera>(
            @"select p.id, p.fecha, p.total, p.usuario_id as UsuarioId, u.nombre, u.email
              from pedidos p
              join usuarios u on u.id = p.usuario_id
              where p.id = @id", new { id });

        if (p == null)
            return NotFound(new { error = "pedido no encontrado" });

        // un usuario solo ve sus pedidos; el admin ve todos
        var esAdmin = HttpContext.Session.GetString("rol") == "admin";
        if (!esAdmin && p.UsuarioId != HttpContext.Session.GetInt32("id"))
            return StatusCode(403, new { error = "no puedes ver este pedido" });

        var detalle = await con.QueryAsync<DetalleTicket>(
            @"select c.marca, c.modelo, d.cantidad, d.precio_unitario as PrecioUnitario
              from detalle_pedido d
              join celulares c on c.id = d.celular_id
              where d.pedido_id = @id", new { id });

        // los nombres de abajo son los que espera ticket.html
        return Ok(new
        {
            id = p.Id,
            fecha = p.Fecha,
            total = p.Total,
            nombre = p.Nombre,
            email = p.Email,
            items = detalle.Select(d => new
            {
                marca = d.Marca,
                modelo = d.Modelo,
                cantidad = d.Cantidad,
                precio_unitario = d.PrecioUnitario
            })
        });
    }
}