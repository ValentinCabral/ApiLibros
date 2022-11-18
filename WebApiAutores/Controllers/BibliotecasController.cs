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

        [HttpGet]
        public async Task<ActionResult<BibliotecaDTO>> VerMisLibros()
        {
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();

            if(biblioteca.Libros.Count() == 0)
            {
                return NotFound();
            }

            return mapper.Map<BibliotecaDTO>(biblioteca);
        }

        [HttpPost("{libroId:int}")]
        public async Task<ActionResult> AgregarLibro([FromRoute] int libroId)
        {
            var libro = await context.Libros.Where((x => x.Id == libroId)).FirstOrDefaultAsync();
            
            if (libro is null)
                return NotFound();

            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var biblioteca = await context.Bibliotecas.Include(x => x.Libros).Where(x => x.UsuarioId == usuarioId).FirstOrDefaultAsync();


            if(biblioteca is null)
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
                if (biblioteca.Libros.Any(x => x.Id == libroId))
                {
                    return BadRequest("Este libro ya esta en tu biblioteca");
                }
                else
                {
                    biblioteca.Libros.Add(libro);
                }
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
