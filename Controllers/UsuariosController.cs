using CellStore.Data;
using CellStore.Filters;
using CellStore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CellStore.Controllers;

[ApiController]
[Route("api/usuarios")]
[RequiereAdmin]
public class UsuariosController : ControllerBase
{
    private readonly Db _db;
    public UsuariosController(Db db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        using var con = _db.Conectar();
        var lista = await con.QueryAsync<UsuarioLista>(
            "select id, nombre, email, rol from usuarios order by id");
        return Ok(lista);
    }

    [HttpPut("{id:int}/rol")]
    public async Task<IActionResult> CambiarRol(int id, RolDto dto)
    {
        if (dto.Rol != "admin" && dto.Rol != "usuario")
            return BadRequest(new { error = "datos invalidos" });

        if (id == HttpContext.Session.GetInt32("id"))
            return BadRequest(new { error = "no puedes cambiar tu propio rol" });

        using var con = _db.Conectar();
        var filas = await con.ExecuteAsync(
            "update usuarios set rol = @rol where id = @id",
            new { rol = dto.Rol, id });

        return filas == 0
            ? NotFound(new { error = "usuario no encontrado" })
            : Ok(new { mensaje = "rol actualizado" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (id == HttpContext.Session.GetInt32("id"))
            return BadRequest(new { error = "no puedes eliminar tu propia cuenta" });

        try
        {
            using var con = _db.Conectar();
            var filas = await con.ExecuteAsync(
                "delete from usuarios where id = @id", new { id });

            return filas == 0
                ? NotFound(new { error = "usuario no encontrado" })
                : Ok(new { mensaje = "usuario eliminado" });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return Conflict(new { error = "este usuario tiene pedidos y no se puede eliminar" });
        }
    }
}