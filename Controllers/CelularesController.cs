using CellStore.Data;
using CellStore.Filters;
using CellStore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CellStore.Controllers;

[ApiController]
[Route("api/celulares")]
public class CelularesController : ControllerBase
{
    private readonly Db _db;
    public CelularesController(Db db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        using var con = _db.Conectar();
        var lista = await con.QueryAsync<Celular>(
            "select id, marca, modelo, precio, stock from celulares order by id");
        return Ok(lista);
    }

    [HttpPost, RequiereAdmin]
    public async Task<IActionResult> Crear(Celular c)
    {
        if (string.IsNullOrWhiteSpace(c.Marca) || string.IsNullOrWhiteSpace(c.Modelo) || c.Precio < 0 || c.Stock < 0)
            return BadRequest(new { error = "datos invalidos" });

        using var con = _db.Conectar();
        var id = await con.ExecuteScalarAsync<int>(
            @"insert into celulares (marca, modelo, precio, stock)
              output inserted.id
              values (@Marca, @Modelo, @Precio, @Stock)", c);
        return Created("", new { id });
    }

    [HttpPut("{id:int}"), RequiereAdmin]
    public async Task<IActionResult> Actualizar(int id, Celular c)
    {
        if (string.IsNullOrWhiteSpace(c.Marca) || string.IsNullOrWhiteSpace(c.Modelo) || c.Precio < 0 || c.Stock < 0)
            return BadRequest(new { error = "datos invalidos" });

        using var con = _db.Conectar();
        var filas = await con.ExecuteAsync(
            @"update celulares set marca=@Marca, modelo=@Modelo, precio=@Precio, stock=@Stock
              where id=@id",
            new { c.Marca, c.Modelo, c.Precio, c.Stock, id });
        return filas == 0 ? NotFound(new { error = "celular no encontrado" }) : Ok(new { mensaje = "actualizado" });
    }

    [HttpDelete("{id:int}"), RequiereAdmin]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            using var con = _db.Conectar();
            var filas = await con.ExecuteAsync("delete from celulares where id=@id", new { id });
            return filas == 0 ? NotFound(new { error = "celular no encontrado" }) : Ok(new { mensaje = "eliminado" });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return Conflict(new { error = "este celular tiene pedidos y no se puede borrar" });
        }
    }
}