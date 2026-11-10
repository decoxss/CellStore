using CellStore.Data;
using CellStore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace CellStore.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Db _db;
    public AuthController(Db db) => _db = db;

    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Nombre) ||
            string.IsNullOrWhiteSpace(d.Email) ||
            string.IsNullOrWhiteSpace(d.Password))
            return BadRequest(new { error = "faltan datos" });

        var email = d.Email.Trim();

        using var con = _db.Conectar();
        var existe = await con.ExecuteScalarAsync<int>(
            "select count(*) from usuarios where email = @email", new { email });
        if (existe > 0)
            return Conflict(new { error = "ese correo ya esta registrado" });

        var hash = BCrypt.Net.BCrypt.HashPassword(d.Password);

        await con.ExecuteAsync(
            @"insert into usuarios (nombre, email, password, rol)
              values (@nombre, @email, @hash, 'usuario')",
            new { nombre = d.Nombre.Trim(), email, hash });

        return Created("", new { mensaje = "usuario registrado" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Email) || string.IsNullOrEmpty(d.Password))
            return Unauthorized(new { error = "correo o contraseña incorrectos" });

        using var con = _db.Conectar();
        var u = await con.QueryFirstOrDefaultAsync<Usuario>(
            "select id, nombre, email, password, rol from usuarios where email = @email",
            new { email = d.Email.Trim() });

        if (u == null || !BCrypt.Net.BCrypt.Verify(d.Password, u.Password))
            return Unauthorized(new { error = "correo o contraseña incorrectos" });

        HttpContext.Session.SetInt32("id", u.Id);
        HttpContext.Session.SetString("nombre", u.Nombre);
        HttpContext.Session.SetString("rol", u.Rol);

        return Ok(new { id = u.Id, nombre = u.Nombre, rol = u.Rol });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { mensaje = "sesion cerrada" });
    }

    [HttpGet("sesion")]
    public IActionResult Sesion()
    {
        var id = HttpContext.Session.GetInt32("id");
        if (id == null)
            return Unauthorized(new { error = "sin sesion" });

        return Ok(new
        {
            id,
            nombre = HttpContext.Session.GetString("nombre"),
            rol = HttpContext.Session.GetString("rol")
        });
    }
}