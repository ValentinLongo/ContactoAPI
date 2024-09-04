using ContactoAPI.Data;
using ContactoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ContactoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactoController : ControllerBase
    {
        private readonly ContactoContext _context;

        public ContactoController(ContactoContext context)
        {
            _context = context;
        }

        // GET: api/Contacto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contacto>>> GetContactos()
        {
            var contactos = await _context.Contacto.FromSqlRaw("EXEC ObtenerContactos").ToListAsync();

            // Convertir el campo Adjunto a Base64
            foreach (var contacto in contactos)
            {
                if (contacto.Adjunto != null)
                {
                    contacto.AdjuntoNombre = Convert.ToBase64String(contacto.Adjunto);
                }
            }

            return contactos;
        }

        // GET: api/Contacto/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contacto>> GetContacto(int id)
        {
            var contacto = await _context.Contacto
                .FromSqlRaw("EXEC ObtenerContactoPorId @Id", new SqlParameter("@Id", id))
                .FirstOrDefaultAsync();

            if (contacto == null)
            {
                return NotFound();
            }

            if (contacto.Adjunto != null)
            {
                using (var compressedStream = new MemoryStream(contacto.Adjunto))
                {
                    using (var decompressedStream = new MemoryStream())
                    {
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                        contacto.Adjunto = decompressedStream.ToArray();
                    }
                }
            }

            return contacto;
        }

        // POST: api/Contacto
        [HttpPost]
        public async Task<ActionResult<Contacto>> PostContacto([FromForm] Contacto contacto, IFormFile adjunto)
        {
            byte[] fileBytes = null;

            if (adjunto != null && adjunto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await adjunto.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }

            var parameters = new[]
            {
        new SqlParameter("@Email", contacto.Email),
        new SqlParameter("@Nombres", contacto.Nombres),
        new SqlParameter("@Apellidos", contacto.Apellidos),
        new SqlParameter("@Comentarios", contacto.Comentarios),
        new SqlParameter("@Adjunto", (object)fileBytes ?? DBNull.Value),
        new SqlParameter("@AdjuntoNombre", adjunto?.FileName ?? (object)DBNull.Value)
    };

            try
            {
                var idResult = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertarContacto @Email, @Nombres, @Apellidos, @Comentarios, @Adjunto, @AdjuntoNombre",
                    parameters
                );

                var newContactoId = (int)idResult;
                contacto.Id = newContactoId;

                return CreatedAtAction(nameof(GetContacto), new { id = contacto.Id }, contacto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al guardar el contacto: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutContacto(int id, [FromForm] Contacto contacto, IFormFile adjunto = null)
        {
            byte[] fileBytes = null;
            string adjuntoNombre = contacto.AdjuntoNombre;

            if (adjunto != null && adjunto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await adjunto.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                    adjuntoNombre = adjunto.FileName;
                }
            }
            else
            {
                var existingContacto = await _context.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                if (existingContacto != null)
                {
                    fileBytes = existingContacto.Adjunto;
                    adjuntoNombre = existingContacto.AdjuntoNombre;
                }
            }

            var parameters = new[]
            {
        new SqlParameter("@Id", id),
        new SqlParameter("@Email", contacto.Email),
        new SqlParameter("@Nombres", contacto.Nombres),
        new SqlParameter("@Apellidos", contacto.Apellidos),
        new SqlParameter("@Comentarios", contacto.Comentarios),
        new SqlParameter("@Adjunto", SqlDbType.VarBinary) { Value = (object)fileBytes ?? DBNull.Value },
        new SqlParameter("@AdjuntoNombre", (object)adjuntoNombre ?? DBNull.Value)
    };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC ActualizarContacto @Id, @Email, @Nombres, @Apellidos, @Comentarios, @Adjunto, @AdjuntoNombre",
                    parameters
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar el contacto: {ex.Message}" });
            }

            return NoContent();
        }


        // DELETE: api/Contacto/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContacto(int id)
        {
            var parameter = new SqlParameter("@Id", id);
            await _context.Database.ExecuteSqlRawAsync("EXEC EliminarContacto @Id", parameter);

            return NoContent();
        }

        private bool ContactoExists(int id)
        {
            return _context.Contacto.Any(e => e.Id == id);
        }
    }
}
