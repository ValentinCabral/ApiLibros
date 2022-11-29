using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/bibliotecas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BibliotecasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public BibliotecasController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        /// <summary>
        /// Devuelve la lista de todos los libros que esten en la biblioteca del usuario
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<LibroDTO>>> VerMisLibros()
        {

            // Obtengo el email del usuario a partir de los claims del JWT
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;


            // Busco el usuario con ese email
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;


            // Traigo la biblioteca de ese usuario incluyendo los libros
            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();


            // Si no tiene ningún libro la biblioteca es nula
            if (biblioteca.Libros.Count() == 0 || biblioteca is null)
            {
                return NotFound();
            }

            // Si tiene libros mapeo la biblioteca para mostrar solo los libros de la misma
            return mapper.Map<List<LibroDTO>>(biblioteca.Libros);
        }

        /// <summary>
        /// Permite agregar un libro a la biblioteca del usuario logueado
        /// </summary>
        /// <param name="libroId">Id del libro a agregar</param>
        /// <returns></returns>
        [HttpPost("agregarLibro/{libroId:int}")]
        public async Task<ActionResult> AgregarLibro([FromRoute] int libroId)
        {

            // Traigo el libro a agregar en la biblioteca
            var libro = await context.Libros.Where((x => x.Id == libroId)).FirstOrDefaultAsync();

            if (libro is null) // Si no existe ese libro
                return NotFound();

           // Traigo el email de los claim del JWT y obtengo el usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            // Traigo la biblioteca del usuario incluyendo los libros
            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();


            // Si la biblioteca no existia la creo agregando el libro
            if (biblioteca is null)
            {
                context.Bibliotecas.Add(new Biblioteca
                {
                    Usuario = usuario,
                    UsuarioId = usuarioId,
                    Libros = new List<Libro> { libro }
                });
            }
            else
            {
                // Si ya existia la biblioteca veo que no exista el libro a agregar en la misma
                if (biblioteca.Libros.Any(x => x.Id == libroId))
                {
                    return BadRequest("Este libro ya esta en tu biblioteca");
                }
                else
                {
                    // Si no existia el libro lo agrego
                    biblioteca.Libros.Add(libro);
                }
            }

            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }


        /// <summary>
        /// Permite agregar varios libros a la biblioteca a la vez
        /// </summary>
        /// <param name="idsLibros">Lista(arreglo) de los ids de los libros a agregar</param>
        /// <returns></returns>
        [HttpPost("/agregarLibros")]
        public async Task<ActionResult> AgregarLibros(List<int> idsLibros)
        {

            // Traigo los libros a agregar en la biblioteca
            var libros = await context.Libros.Where(x => idsLibros.Contains(x.Id)).ToListAsync();

            // Si no existen 
            if (libros is null || libros.Count == 0)
            {
                return NotFound();
            }

            // Traigo el email del usuario desde los claims del JWT y obtengo el usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            // Traigo la biblioteca del usuario
            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();

            if (biblioteca is null) // Si no existia la biblioteca la creo con los nuevos libros
            {
                context.Bibliotecas.Add(new Biblioteca
                {
                    Usuario = usuario,
                    UsuarioId = usuarioId,
                    Libros = libros
                });
            }
            else
            {
                // Si ya existia agrego cada libro en la biblioteca
                foreach (var libro in libros)
                {
                    if (biblioteca.Libros.Any(x => x.Id == libro.Id))
                    {
                        return BadRequest($"El libro de Id {libro.Id} ya esta en la biblioteca"); // Controlo que el libro que agrego no exista en la biblioteca
                    }
                    else
                    {
                        biblioteca.Libros.Add(libro);
                    }
                }
            }
            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Permite borrar un libro de la biblioteca del usuario
        /// </summary>
        /// <param name="idLibro">Id del libro a borrar</param>
        /// <returns></returns>
        [HttpDelete("borrar/{idLibro:int}")]
        public async Task<ActionResult> Delete(int idLibro)
        {
            // Obtengo el email de los claims del JWT y traigo el usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            // Traigo la biblioteca de ese usuario
            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (biblioteca is null) // Si la biblioteca esta vacia no puedo borrar ningún libro
            {
                return BadRequest("Este usuario no tiene libros en su biblioteca");
            }

            // Traigo el libro que quiero borrar de la biblioteca
            var libro = biblioteca.Libros.FirstOrDefault(x => x.Id == idLibro);

            if (libro is null) // Si el libro no existia en la biblioteca no puedo borrarlo
            {
                return BadRequest();
            }

            // Traigo todos los libros de la biblioteca
            var bibliotecaLibros = biblioteca.Libros;
            // Borro el libro
            bibliotecaLibros.Remove(libro);
            // Actualizo los libros de la biblioteca
            biblioteca.Libros = bibliotecaLibros;

            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Permite borrar una lista de libros de la biblioteca del usuario
        /// </summary>
        /// <param name="idsLibros">Lista con los ids de los libros a borrar</param>
        /// <returns></returns>
        [HttpDelete("borrarLibros")]
        public async Task<ActionResult> BorrarLibros(List<int> idsLibros)
        {   
            // Obtengo el email de los claims del JWT y traigo el usuario
            var emailCLaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailCLaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            // Traigo la biblioteca del usuario
            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            // Traigo los libros de la biblioteca
            var bibliotecaLibros = biblioteca.Libros;
            if (biblioteca is null || bibliotecaLibros is null) // Si la biblioteca no tiene libros
                return BadRequest("Este usuario no tiene libros en su biblioteca");

            // Traigo los libros con los ID's que se quieren borrar
            var libros = await context.Libros.Where(x => idsLibros.Contains(x.Id)).ToListAsync();

            if (libros is null) // Si no existen esos libros
                return BadRequest();

            foreach (var libro in libros)
            {
                // Si alguno de los libros a borrar no existia en la biblioteca
                if ( !bibliotecaLibros.Any(x => x.Id == libro.Id))
                    return BadRequest($"El libro de id {libro.Id} no existe");
                
                // Borro cada libro de la biblioteca
                bibliotecaLibros.Remove(libro);
            }
            // Actualizo la nueva biblioteca con los libros borrados
            biblioteca.Libros = bibliotecaLibros;
            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
