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

        [HttpPost("registro")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuarios usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que no exista el correo
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == usuario.Correo);
            if (usuarioExistente != null)
                return BadRequest("Ya existe un usuario con ese correo.");

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Crear automáticamente un cliente relacionado
            var cliente = new Clientes
            {
                IdUsuario = usuario.IdUsuario, // Usa la PK generada
                NombreCompleto = usuario.NombreUsuario,
                Contacto = usuario.Correo
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado y cliente creado correctamente", usuario.IdUsuario });
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
