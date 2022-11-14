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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        /// Hace un mapeo a AutorDTO, mostrando solo los id's, nombres, apellidos, y libros.
        /// </summary>
        /// <param name="ordenarPor">1: Nombre, 2: Apellido, 3: Fecha de nacimiento</param>
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

        [HttpGet("{id:int}", Name = "ObtenerAutorPorId")]
        [AllowAnonymous]
        /*
         * Este método permite buscar un Autor por su id
         * Si no existe ninguno con el Id recibido retorna un NotFound
         * Y lo devuelve en forma AutorDTO (Id, Nombre, Apellido y Libros)
        */
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

        [HttpGet("nombre/{nombre}")]
        [AllowAnonymous]
        /*
         * Este método permite buscar un Autor por su nombre
         * Si no existe ninguno con el Nombre recibido retorna NotFound
         * Lo devuelve en forma de AutorDTO (Id, Nombre, Apellido y Libros)
        */
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

        [HttpGet("apellido/{apellido}")]
        [AllowAnonymous]
        /*
         * Este método permite buscar un Autor por su apellido
         * Si no existe ninguno con el Apellido recibido retorna NotFound
         * Lo devuelve en forma de AutorDTO (Id, Nombre y Apellido)
        */
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

        [HttpGet("buscar/{apellidoONombre}")]
        [AllowAnonymous]
        /*
         * Este método permite buscar un autor que contenga el string recibido ya sea en su nombre o en su apellido
         * Si no existe ninguno con ese string en el Nombre o en el Apellido retorna NotFound
         * Lo devuelve en forma de AutorDTO (Id, Nombre, Apellido)
        */
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

        [HttpPost]
        /*
         * Este método permite crear un nuevo Autor
         * Lo recibe en forma de AutorCreacionDTO (Nombre, Apellido)
        */
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

        [HttpPut("{id:int}")]
        /*
         * Este método permite actualizar los datos de un Autor ya existente
         * Recibe un AutorDTO (Id, Nombre y Apellido) y un Id
         * En caso de que no exista un autor con ese Id retorna un NotFound
         * En caso de que exista, pero que el Id del autor recibido no coincida con el id de la URL
         * Devuelve un BadRequest
        */
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

        [HttpDelete("{id:int}")]
        /*
         * Este método permite borrar un Autor ya existente dado su Id
        */
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id); // Booleano, existe algún autor con ese Id
            if (!existe) // Si no existe ninguno
                return NotFound();

            context.Remove(new Autor() { Id = id}); // Elimino el autor con ese Id
            await context.SaveChangesAsync(); // Persito y guardo los cambios en la BD
            return Ok();
        }

        [HttpPatch("{id:int}")]
        /*
         * Este método permite actualizar un autor parcialmente (Un campo especifico)
        */
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
