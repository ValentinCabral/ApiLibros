using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        /// <summary>
        /// Acceder a todos los comentarios de un libro
        /// </summary>
        /// <param name="libroId">Id del libro que tiene los comentarios</param>
        /// <returns>Listado de comentarios</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ComentarioDTO>>> Get([FromRoute] int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id

            if (!existeLibro) // No existe ningún libro con ese Id
                return NotFound();

            var comentarios = await context.Comentarios.Where(x => x.LibroId == libroId).ToListAsync(); // Lista de todos los comentarios que tengan el LibroId

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        /// <summary>
        /// Buscar un comentario por su ID
        /// </summary>
        /// <param name="libroId">Id del libro que tiene el comentario</param>
        /// <param name="id">Id del comentario</param>
        /// <returns>Un comentario</returns>
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


        /// <summary>
        /// Buscar los comentarios de un usuario especifico
        /// </summary>
        /// <param name="libroId">id del libro que tiene los comentarios</param>
        /// <returns>Lista de comentarios del usuario en ese libro</returns>
        [HttpGet("/Usuario/{libroId:int}")]
        public async Task<ActionResult<List<ComentarioDTO>>> GetPorUsuario([FromRoute] int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id

            if (!existeLibro) // No existe ningún libro con ese Id
                return BadRequest("No existe un libro con ese ID");

            // Traigo los claims de tipo email
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            // Obtengo el valor del claim
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var comentarios = await context.Comentarios
                .Where(x => x.UsuarioId == usuarioId)
                .Where(x => x.LibroId == libroId)
                .ToListAsync();

            if (comentarios is null)
                return NotFound();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }


        /// <summary>
        /// Crear un nuevo comentario
        /// </summary>
        /// <param name="libroId">Id del libro que se quiere comentar</param>
        /// <param name="comentarioCreacionDTO">Texto del comentario</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post([FromRoute] int libroId,[FromBody] ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId); // Booleano, si existe algún libro con ese Id

            if (!existeLibro) // No existe ningún libro con ese Id
                return NotFound();

            // Traigo los claims de tipo email
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            // Obtengo el valor del claim
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO); // Mapeo desde ComentarioCreacionDTO a Comentario
            comentario.LibroId = libroId; // Guardo el libroId ya que ComentarioCreacionDTO no tenia ese campo
            comentario.UsuarioId = usuarioId;

            context.Comentarios.Add(comentario); // Agrego el comentario
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return CreatedAtRoute("ObtenerComentarioPorId", new {libroId = libroId, id = comentario.Id}, comentarioCreacionDTO);
        }

      
        /// <summary>
        /// Actualizar un comentario especifico
        /// </summary>
        /// <param name="libroId">Id del libro que tiene el comentario</param>
        /// <param name="id">Id del comentario</param>
        /// <param name="comentarioCreacionDTO">Nuevo contenido del comentario</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
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

        /// <summary>
        /// Borrar un comentario (Solo los usuarios autenticados pueden borrar, y solo pueden borrar sus propios comentarios)
        /// </summary>
        /// <param name="libroId">Id del libro que tiene el comentario</param>
        /// <param name="id">Id del comentario</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int libroId, [FromRoute] int id)
        {

            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro) // No existe ningún libro con ese Id
                return BadRequest("No existe ningún libro con ese Id");

            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);

            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (!existeComentario) // No existe ningún comentario con ese Id
                return BadRequest("Este usuario no tiene ningún comentario con ese Id");

            context.Remove(new Comentario { Id = id });
            await context.SaveChangesAsync();
            return Ok();


        }
    }
}
