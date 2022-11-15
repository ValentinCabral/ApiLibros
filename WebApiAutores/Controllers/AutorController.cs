using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [Route("api/autores")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutorController : ControllerBase
    {
        
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOrdenarAutores ordenarAutores;

        public AutorController(ApplicationDbContext context, IMapper mapper, IOrdenarAutores ordenarAutores)
        {
            this.context = context;
            this.mapper = mapper;
            this.ordenarAutores = ordenarAutores;
        }


        /// <summary>
        /// Este método permite obtener un listado de todos los Autores
        /// </summary>
        /// <param name="ordenarPor">1: Nombre, 2: Apellido, 3: Fecha de nacimiento, Otros: Id</param>
        /// <param name="tipoOrden">1: Ascendente, 2: Descendente</param>
        /// <returns>Listado de autores ordenados segun lo especificado</returns>

        [HttpGet("{ordenarPor?}/{tipoOrden?}")]
        [AllowAnonymous]
    
        public async Task<ActionResult<List<AutorDTO>>> GetListado(int ordenarPor,int tipoOrden)
        {
            //var autores = await context.Autores
            //    .Include(x => x.AutoresLibros)
            //    .ThenInclude(autorLibro => autorLibro.Libro)  // Incluyo cada libro de AutoresLibros
            //    .ToListAsync(); // Obtengo la lista de todos los autores

            if(ordenarPor == 0 && tipoOrden == 0)
            {
                tipoOrden = 1;
            }

            var autores = await ordenarAutores.TraerAutores(ordenarPor,tipoOrden);

            if (autores is null)
                return NotFound();

            return mapper.Map<List<AutorDTO>>(autores); // Mapeo desde Autor a AutorDTO y retorno
        }


        /// <summary>
        /// Buscar un Autor por su Id
        /// </summary>
        /// <param name="id">Id del autor a buscar</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "ObtenerAutorPorId")]
        [AllowAnonymous]

        public async Task<ActionResult<AutorDTO>> GetPorId([FromRoute] int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);  // Booleano, si existe alguno con el Id == id

            if (!existe)
                return NotFound(); // No encontrado

            var autor = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro) // Incluyo los libros
                .FirstOrDefaultAsync(x => x.Id == id); // El primero o el unico con el Id == id


            return mapper.Map<AutorDTO>(autor); // Mapeo a AutorDto y retorno
        }

        /// <summary>
        /// Buscar un Autor por su nombre
        /// </summary>
        /// <param name="nombre">Nombre que se quiere buscar entre los autores</param>
        /// <returns></returns>
        [HttpGet("nombre/{nombre}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var listaAutores = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync(); // Lista de todos los autores

            listaAutores.RemoveAll(x => !(x.Nombre.ToLower().Contains(nombre.ToLower()))); // Saco de la lista los que no contengan el string en el nombre

            if(listaAutores.Count() == 0)
                return NotFound(); // Si no existe ninguno con ese nombre



            return mapper.Map<List<AutorDTO>>(listaAutores); // Mapeo y retorno
        }

        /// <summary>
        /// Buscar un Autor por su apellido
        /// </summary>
        /// <param name="apellido">Apellido a buscar entre los autores</param>
        /// <returns></returns>
        [HttpGet("apellido/{apellido}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> GetPorApellido([FromRoute] string apellido)
        {
            var listaAutores = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync(); // Lista de todos los autores
            listaAutores.RemoveAll(x => !(x.Apellido.ToLower().Contains(apellido.ToLower()))); // Saco de la lista los que no coincidan en el apellido

            if(listaAutores.Count() == 0)
                return NotFound(); // Si no existe ninguno con ese apellido
            
            return mapper.Map<List<AutorDTO>>(listaAutores); // Mapeo y retorno
        }

        /// <summary>
        /// Buscar cualquier Autor que contenga el string recibido en su nombre o en su apellido
        /// </summary>
        /// <param name="apellidoONombre">Palabra a buscar en el nombre o apellido de los autores</param>
        /// <returns></returns>
        [HttpGet("buscar/{apellidoONombre}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombreOrApellido ([FromRoute] string apellidoONombre)
        {
            var listaAutoresNombre = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync(); //Lista de todos los autores
            listaAutoresNombre.RemoveAll(x => !(x.Nombre.ToLower().Contains(apellidoONombre.ToLower()))); // Remuevo todos los que no coincidan en el Nombre

            var listaAutoresApellido = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync(); // Lista de todos los autores
            listaAutoresApellido.RemoveAll(x => !(x.Apellido.ToLower().Contains(apellidoONombre.ToLower()))); // Remuevo todos los que no coincidan en el Apellido

            if(listaAutoresNombre.Count() > 0) // Si en la lista de AutoresNombre quedo alguno (hubo coincidencia)
                return mapper.Map<List<AutorDTO>>(listaAutoresNombre); // Mapeo y retorno

            if (listaAutoresApellido.Count() > 0) // Si en la lista de AutoresApellido quedo alguno (hubo coincidencia)
                return mapper.Map<List<AutorDTO>>(listaAutoresApellido); // Mapeo y retorno

            return NotFound();
        }

        /// <summary>
        /// Permite crear un nuevo Autor
        /// </summary>
        /// <param name="autorDTO">Datos del Autor</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorDTO)
        {
            // Si ya existe algún autor con ese nombre y apellido
            var yaExisteAutor = await context.Autores.AnyAsync(x => x.Nombre == autorDTO.Nombre && x.Apellido == autorDTO.Apellido);

            if (yaExisteAutor) // Si ya existe 
                return BadRequest($"Ya existe un autor con el nombre {autorDTO.Nombre} {autorDTO.Apellido}");

            var autor = mapper.Map<Autor>(autorDTO); // Mapeo el AutorDTO a Autor
            context.Add(autor); // Lo agrego
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return CreatedAtRoute("ObtenerAutorPorId", new {id = autor.Id}, autorDTO);
        }

        /// <summary>
        /// Actualizar los datos de un Autor ya existente
        /// </summary>
        /// <param name="autorCreacionDTO">Id, Nombre y Apellido del Autor</param>
        /// <param name="id">Busca el autor con ese Id y verifica que sea el mismo que se recibe</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromBody] AutorCreacionDTO autorCreacionDTO, [FromRoute] int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id); // Booleano, algún autor con ese Id

            if (!existe) // No existe ninguno con ese Id
                return NotFound();

            var autor = mapper.Map<Autor>(autorCreacionDTO); // Mapeo desde autorDTO a Autor
            autor.Id = id;
            context.Update(autor); // Actualizo
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return Ok();
        }

        /// <summary>
        /// Borrar un Autor
        /// </summary>
        /// <param name="id">Id del Autor a borrar</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id); // Booleano, existe algún autor con ese Id
            if (!existe) // Si no existe ninguno
                return NotFound();

            context.Remove(new Autor() { Id = id}); // Elimino el autor con ese Id
            await context.SaveChangesAsync(); // Persito y guardo los cambios en la BD
            return Ok();
        }

        /// <summary>
        /// Actualizar parcialmente,es decir un campo especifico, de un Autor
        /// </summary>
        /// <param name="id">Id del Autor a actualizar</param>
        /// <param name="patchDocument">source del Campo, operacion (replace) y valor a actualizar</param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch([FromRoute] int id, JsonPatchDocument<AutorCreacionDTO> patchDocument)
        {
            if (patchDocument is null) // El jsonPatch que me mandan no es valido
                return BadRequest();
            
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null) // Si no existe el autor con ese Id
                return NotFound();

            var autorDTO = mapper.Map<AutorCreacionDTO>(autor);


            // Aplico los cambios que vienen desde el jsonPatch en el autorDTO
            patchDocument.ApplyTo(autorDTO, ModelState); // Paso el modelstate para guardar los errores en caso de que existan

            var esValido = TryValidateModel(autorDTO); // Compruebo que todo lo que quedo en AutorDTO es valido

            if (!esValido)
                return BadRequest();

            // Hago el mapeo para guardar los cambios
            mapper.Map(autorDTO, autor);

            await context.SaveChangesAsync(); // Guardo los cambios en la BD
            return NoContent();
        }
    }
}
