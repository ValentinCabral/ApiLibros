using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Migrations;

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

            // Obtengo el mail del usuario
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            // Traigo el usuario
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var biblioteca = await context.Bibliotecas
                .Include(biblioteca => biblioteca.LibrosBibliotecas)
                .ThenInclude(libroBiblioteca => libroBiblioteca.Libro)
                .Where(biblioteca => biblioteca.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (biblioteca.LibrosBibliotecas.Count == 0 || biblioteca is null)
                return NotFound();

            var librosBibliotecas = biblioteca.LibrosBibliotecas.ToList();

            var libros = new List<Libro>();

            foreach (var libroBiblioteca in librosBibliotecas)
            {
                libros.Add(libroBiblioteca.Libro);
            }

            return mapper.Map <List<LibroDTO>>(libros);

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
            var libro = await context.Libros
                .Include(x => x.AutoresLibros)
                .Where((x => x.Id == libroId))
                .FirstOrDefaultAsync();

            if (libro is null) // Si no existe ese libro
                return NotFound();

            // Traigo el email de los claim del JWT y obtengo el usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            // Traigo la biblioteca del usuario incluyendo los libros
            var biblioteca = await context.Bibliotecas
                .Include(x => x.LibrosBibliotecas)
                .Where(x => x.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();


            // Si la biblioteca no existia la creo agregando el libro
            if (biblioteca is null)
            {
                context.Bibliotecas.Add(new Biblioteca
                {
                    Usuario = usuario,
                    UsuarioId = usuarioId,
                    LibrosBibliotecas = new List<LibroBibliotecas>()
                    {
                        new LibroBibliotecas(){Libro = libro, LibroId = libroId}
                    }
                });

                // Guardo los cambios en la base de datos para que la biblioteca ya tenga un ID y la vuelvo a traer
                await context.SaveChangesAsync();

                biblioteca = await context.Bibliotecas
                .Include(x => x.LibrosBibliotecas)
                .Where(x => x.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

                biblioteca.LibrosBibliotecas.ForEach(x => x.Biblioteca = biblioteca);
                biblioteca.LibrosBibliotecas.ForEach(x => x.BibliotecaId = biblioteca.Id);
                await context.SaveChangesAsync();

            }
            else
            {
                var librosBibliotecas = biblioteca.LibrosBibliotecas;
                // Si ya existia la biblioteca veo que no exista el libro a agregar en la misma
                if (librosBibliotecas.Any(x => x.LibroId == libroId))
                {
                    return BadRequest("Este libro ya esta en tu biblioteca");
                }
                else
                {
                    var nuevoLibroBiblioteca = new LibroBibliotecas()
                    {
                        Libro = libro,
                        LibroId = libroId,
                        Biblioteca = biblioteca,
                        BibliotecaId = biblioteca.Id
                    };
                    // Si no existia el libro lo agrego
                    biblioteca.LibrosBibliotecas.Add(nuevoLibroBiblioteca);
                }
            }

            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }


        //    /// <summary>
        //    /// Permite agregar varios libros a la biblioteca a la vez
        //    /// </summary>
        //    /// <param name="idsLibros">Lista(arreglo) de los ids de los libros a agregar</param>
        //    /// <returns></returns>
        //    [HttpPost("/agregarLibros")]
        //    public async Task<ActionResult> AgregarLibros(List<int> idsLibros)
        //    {

        //        // Traigo los libros a agregar en la biblioteca
        //        var libros = await context.Libros.Where(x => idsLibros.Contains(x.Id)).ToListAsync();

        //        // Si no existen 
        //        if (libros is null || libros.Count == 0)
        //        {
        //            return NotFound();
        //        }

        //        // Traigo el email del usuario desde los claims del JWT y obtengo el usuario
        //        var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
        //        var email = emailClaim.Value;

        //        var usuario = await userManager.FindByEmailAsync(email);
        //        var usuarioId = usuario.Id;

        //        // Traigo la biblioteca del usuario
        //        var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();

        //        if (biblioteca is null) // Si no existia la biblioteca la creo con los nuevos libros
        //        {
        //            context.Bibliotecas.Add(new Biblioteca
        //            {
        //                Usuario = usuario,
        //                UsuarioId = usuarioId,
        //                Libros = libros
        //            });
        //        }
        //        else
        //        {
        //            // Si ya existia agrego cada libro en la biblioteca
        //            foreach (var libro in libros)
        //            {
        //                if (biblioteca.Libros.Any(x => x.Id == libro.Id))
        //                {
        //                    return BadRequest($"El libro de Id {libro.Id} ya esta en la biblioteca"); // Controlo que el libro que agrego no exista en la biblioteca
        //                }
        //                else
        //                {
        //                    biblioteca.Libros.Add(libro);
        //                }
        //            }
        //        }
        //        // Guardo los cambios en la BD
        //        await context.SaveChangesAsync();
        //        return Ok();
        //    }

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
            var biblioteca = await context.Bibliotecas
                .Include(x => x.LibrosBibliotecas)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (biblioteca is null || biblioteca.LibrosBibliotecas.Count == 0) // Si la biblioteca esta vacia no puedo borrar ningún libro
            {
                return BadRequest("Este usuario no tiene libros en su biblioteca");
            }

            // Traigo el libro que quiero borrar de la biblioteca
            var libroBiblioteca = biblioteca.LibrosBibliotecas.FirstOrDefault(x => x.Libro.Id == idLibro);

            if (libroBiblioteca is null) // Si el libro no existia en la biblioteca no puedo borrarlo
            {
                return BadRequest();
            }

            // Borro el libroBiblioteca que tiene ese libro
            biblioteca.LibrosBibliotecas.Remove(libroBiblioteca);

            // Guardo los cambios en la BD
            await context.SaveChangesAsync();
            return Ok();
        }

        //    /// <summary>
        //    /// Permite borrar una lista de libros de la biblioteca del usuario
        //    /// </summary>
        //    /// <param name="idsLibros">Lista con los ids de los libros a borrar</param>
        //    /// <returns></returns>
        //    [HttpDelete("borrarLibros")]
        //    public async Task<ActionResult> BorrarLibros(List<int> idsLibros)
        //    {   
        //        // Obtengo el email de los claims del JWT y traigo el usuario
        //        var emailCLaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
        //        var email = emailCLaim.Value;

        //        var usuario = await userManager.FindByEmailAsync(email);
        //        var usuarioId = usuario.Id;

        //        // Traigo la biblioteca del usuario
        //        var biblioteca = await context.Bibliotecas.Include(x => x.Libros).FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

        //        // Traigo los libros de la biblioteca
        //        var bibliotecaLibros = biblioteca.Libros;
        //        if (biblioteca is null || bibliotecaLibros is null) // Si la biblioteca no tiene libros
        //            return BadRequest("Este usuario no tiene libros en su biblioteca");

        //        // Traigo los libros con los ID's que se quieren borrar
        //        var libros = await context.Libros.Where(x => idsLibros.Contains(x.Id)).ToListAsync();

        //        if (libros is null) // Si no existen esos libros
        //            return BadRequest();

        //        foreach (var libro in libros)
        //        {
        //            // Si alguno de los libros a borrar no existia en la biblioteca
        //            if ( !bibliotecaLibros.Any(x => x.Id == libro.Id))
        //                return BadRequest($"El libro de id {libro.Id} no existe");

        //            // Borro cada libro de la biblioteca
        //            bibliotecaLibros.Remove(libro);
        //        }
        //        // Actualizo la nueva biblioteca con los libros borrados
        //        biblioteca.Libros = bibliotecaLibros;
        //        // Guardo los cambios en la BD
        //        await context.SaveChangesAsync();
        //        return Ok();
        //    }
        //}
    }
}
