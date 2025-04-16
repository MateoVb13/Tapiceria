using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        public UsuariosController(TapiceriaContext context)
        {
            _context = context;
        }

        // POST: api/Usuarios/registro
        [HttpPost("registro")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuarios usuario)
        {
            if (usuario == null)
                return BadRequest("Datos inválidos.");

            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);
            if (existeUsuario)
                return Conflict("El nombre de usuario ya existe.");

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuarios credenciales)
        {
            if (credenciales == null)
                return BadRequest("Datos inválidos.");

            // Buscar usuario por nombre y contraseña (en producción: comparar contraseñas hasheadas)
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Correo == credenciales.Correo && u.Contrasena == credenciales.Contrasena);

            if (usuario == null)
                return Unauthorized("Credenciales incorrectas.");

            return Ok(new { Mensaje = "Sesión iniciada correctamente", Usuario = usuario });
        }

        // GET: api/Usuarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuarios>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            return usuario;
        }

        // Otros endpoints: login, actualizar, eliminar...
    }
}
