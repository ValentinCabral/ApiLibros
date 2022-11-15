using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/libros")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class LibroController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibroController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        /// <summary>
        /// Listado de todos los libros
        /// </summary>
        /// <returns>Lista de libros</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<LibroDTO>>> Get()
        {
            var libros = await context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Autor)  // Traigo cada Autor dentro de AutoresLibros
                .ToListAsync();  // Lista con todos los libros

            
            return mapper.Map<List<LibroDTO>>(libros);  // Mapeo desde Libro hasta LibroDTO y retorno
        }

        /// <summary>
        /// Obtener un libro por su Id
        /// </summary>
        /// <param name="id">Id del libro</param>
        /// <returns>Libro</returns>
        [HttpGet("{id:int}", Name = "ObtenerLibroPorId")]
        [AllowAnonymous]
        public async Task<ActionResult<LibroDTO>> GetPorId([FromRoute] int id)
        {
            var libro = await context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Autor) // Traigo el Autor de AutoresLibros
                .FirstOrDefaultAsync(x => x.Id == id); // El que tenga ese id

            if (libro is null) // No existe el libro
                return NotFound();

            return mapper.Map<LibroDTO>(libro);
        }

        /// <summary>
        /// Buscar libros de un autor
        /// </summary>
        /// <param name="autorId">Id del autor</param>
        /// <returns>Los libros de ese autor</returns>
        [HttpGet("buscar-por-autor/{autorId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<LibroMostrarEnAutoresDTO>>> GetPorAutoresId([FromRoute] int autorId)
        {

            var existeAutor = await context.Autores.AnyAsync(x => x.Id == autorId);

            if (!existeAutor) // Si no existe ningún autor con ese Id
                return BadRequest("No existe ningún autor con ese Id");


            // Lista de todos los libros
            var libros = await context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Autor)
                .ToListAsync();

            // Remuevo los libros que no tengan autorId entre sus autores
            libros.RemoveAll(x => !(x.AutoresLibros.Select(x => x.AutorId).Contains(autorId))); 
            
            
            if (libros.Count == 0 || libros is null) //Si no quedo ningún libro con ese autor
                return NotFound();

            return mapper.Map<List<LibroMostrarEnAutoresDTO>>(libros);
        }

        /// <summary>
        /// Buscar libros de uno o varios autores según su nombre
        /// </summary>
        /// <param name="nombre">Nombre del autor</param>
        /// <returns>Listado de libros</returns>
        [HttpGet("buscar-por-autor/{nombre}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<List<LibroMostrarEnAutoresDTO>>>> GetPorAutoresNombre([FromRoute] string nombre)
        {

            // Si existe algún autor que contenga el string en su nombre o apelllido
            var existeAutor = await context.Autores.AnyAsync(x => x.Nombre.ToLower().Contains(nombre.ToLower()) || x.Apellido.ToLower().Contains(nombre.ToLower()));

            if (!existeAutor) // Si no existe ninguno
                return BadRequest("No existe ningún autor con ese nombre");


            // Traigo una lista de todos esos autores
            var autores = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro)
                .Where(x => x.Nombre.ToLower().Contains(nombre.ToLower()) || x.Apellido.ToLower().Contains(nombre.ToLower()))
                .ToListAsync();


            // Para poder traer los Libros a partir de los autores necesito traer AutorLibro (AutorId, LibroId, Autor, Libro)
            var autoresLibros = autores.Select(x => x.AutoresLibros).ToList();
            // A partir de esos autoresLibros traigo una lista con los libros de cada uno
            var libros = autoresLibros.Select(x => x.Select(x => x.Libro)).ToList();
          
            if (libros.Count == 0 || libros is null)
                return NotFound();


            // Voy a tener una lista de una lista ya que traigo la lista de autoresLibros y a partir de esa lista traigo la lista de libros.
            return mapper.Map<List<List<LibroMostrarEnAutoresDTO>>>(libros);
        }

        /// <summary>
        /// Buscar libros según su titulo
        /// </summary>
        /// <param name="titulo">String a buscar en los titulos de los libros</param>
        /// <returns>Listado de libros</returns>
        [HttpGet("{titulo}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<LibroDTO>>> BuscarPorTitulo([FromRoute] string titulo)
        {
            var libros = await context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Autor)
                .ToListAsync();

            libros.RemoveAll(x => !x.Titulo.ToLower().Contains(titulo.ToLower()));

            if (libros.Count == 0)
                return NotFound();

            return mapper.Map<List<LibroDTO>>(libros);
        }

        /// <summary>
        /// Crear un nuevo libro
        /// </summary>
        /// <param name="libroDTO">Datos del libro</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroDTO)
        {
            if (libroDTO.AutoresIds is null || libroDTO.AutoresIds.Count == 0)  // Si no me pasan ningún autor
                return BadRequest("No se puede crear un libro sin autores");

            // Lista de autores
            var autores = await context.Autores 
                .Where(x => libroDTO.AutoresIds.Contains(x.Id)) // El id del autor se encuentre en el listado de autores recibidos
                .Select(x => x.Id)  // Traer solamente los Id, no todos los datos
                .ToListAsync();

            // Si alguno de los autores enviados no esta en la base de datos entonces autores va a tener menos elementos que libroDTO
            if (libroDTO.AutoresIds.Count != autores.Count)
                return BadRequest("No existe uno de los autores enviados");

            var libro = mapper.Map<Libro>(libroDTO); // Mapeo desde LibroCreacionDTO hasta Libro
            context.Libros.Add(libro); // Agrego el libro
            await context.SaveChangesAsync(); // Persisto y guardo los datos en la BD
            return CreatedAtRoute("ObtenerLibroPorId", new { id = libro.Id}, libroDTO);
        }

        /// <summary>
        /// Actualizar un libro ya existente
        /// </summary>
        /// <param name="libroCreacionDTO">Nuevos datos del libro</param>
        /// <param name="id">Id del libro</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]

        public async Task<ActionResult> Put([FromBody] LibroCreacionDTO libroCreacionDTO,[FromRoute] int id)
        {
            var libro = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
                return NotFound(); // El libro no existe

            libro = mapper.Map(libroCreacionDTO, libro); //Actualizo las propiedades de libro con las de libroCreacionDTO y lo guardo en libro

            await context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Eliminar un libro
        /// </summary>
        /// <param name="id">Id del libro</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id); // Booleano, existe algún libro con ese Id

            if (!existe) // Si no existe ninguno
                return NotFound();

            context.Libros.Remove(new Libro() { Id = id }); // Remuevo el libro con ese Id
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return Ok();
        }

        /// <summary>
        /// Actualizar un dato especifico de un libro
        /// </summary>
        /// <param name="id">Id del libro</param>
        /// <param name="patchDocument">Source del campo, operacion (replace) y nuevo valor</param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch([FromRoute] int id, JsonPatchDocument<LibroCreacionDTO> patchDocument)
        {
            if (patchDocument is null) // El jsonPatch que me mandan no es valido
                return BadRequest();

            var libro = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null) // Si no existe el autor con ese Id
                return NotFound();

            var libroDTO = mapper.Map<LibroCreacionDTO>(libro);


            // Aplico los cambios que vienen desde el jsonPatch en el autorDTO
            patchDocument.ApplyTo(libroDTO, ModelState); // Paso el modelstate para guardar los errores en caso de que existan

            //var esValido = TryValidateModel(libroDTO); // Compruebo que todo lo que quedo en AutorDTO es valido

            //if (!esValido)
            //    return BadRequest();

            // Hago el mapeo para guardar los cambios
            mapper.Map(libroDTO, libro);

            await context.SaveChangesAsync(); // Guardo los cambios en la BD
            return NoContent();
        }
    }
}
