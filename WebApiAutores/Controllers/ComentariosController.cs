using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ComentariosController:ControllerBase 
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        /*
         * Método que permite acceder a todos los comentarios de un libro
         * dado el id del libro
        */
        public async Task<ActionResult<List<ComentarioDTO>>> Get([FromRoute] int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id

            if (!existeLibro) // No existe ningún libro con ese Id
                return NotFound();

            var comentarios = await context.Comentarios.Where(x => x.LibroId == libroId).ToListAsync(); // Lista de todos los comentarios que tengan el LibroId

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet("{id:int}", Name = "ObtenerComentarioPorId")]
        [AllowAnonymous]
        /*
         * Método que permite acceder a un comentario en particular dado su Id de un libro
        */
        public async Task<ActionResult<ComentarioDTO>> GetPorId([FromRoute] int libroId, [FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id
            if (!existeLibro)
                return NotFound(); //No existe ningún libro con ese Id

            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario is null)
                return NotFound(); // No existe ningún comentario con ese Id

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromRoute] int libroId,[FromBody] ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id

            if (!existeLibro) // No existe ningún libro con ese Id
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO); // Mapeo desde ComentarioCreacionDTO a Comentario
            comentario.LibroId = libroId; // Guardo el libroId ya que ComentarioCreacionDTO no tenia ese campo

            context.Comentarios.Add(comentario); // Agrego el comentario
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return CreatedAtRoute("ObtenerComentarioPorId", new {libroId = libroId, id = comentario.Id}, comentarioCreacionDTO);
        }

      
        [HttpPut("{id:int}")]
        /*
         * Este método me permite actualizar un comentario
         * dado su Id
        */
        public async Task<ActionResult> Put([FromRoute] int libroId, [FromRoute] int id, [FromBody] ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro) // No existe ningún libro con ese Id
                return NotFound();

            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id);
            if (!existeComentario)  // No existe ningún comentario con ese Id
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO); // Mapeo desde ComentarioCreacionDTO hacia Comentario
            
            // Asigno los campos Id y LibroId en comentario ya que ComentarioCreacionDTO no tiene estos campos 
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Update(comentario); // Actualizo
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return Ok();
        }
    }
}
