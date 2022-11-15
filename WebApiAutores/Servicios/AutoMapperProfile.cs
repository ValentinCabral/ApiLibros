using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Servicios
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            // AUTORES 
            CreateMap<Autor, AutorDTO>()
                .ForMember(autorDTO => autorDTO.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros));
            CreateMap<AutorDTO, Autor>();
            CreateMap<AutorCreacionDTO, Autor>().ReverseMap();

            // LIBROS
            CreateMap<Libro, LibroDTO>()
                // Cuando traiga un Autor desde Libro voy a necesitar mapear desde ese Autor a AutorDTO
                /*
                 * Lo que estoy diciendo es:
                 * En libroDTO, para el miembro Autores quiero realizar el mapeo MapLibroDTOAutores
                 * que va a ser un mapeo desde Autor hacia AutorDTO
                */
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<LibroCreacionDTO, Libro>()
                // Configuro el mapeo hacia la propiedad AutoresLibros, ya que dado una lista de int (ids) tengo que generar una lista de AutorLibro
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros))
                 .ReverseMap();
            CreateMap<Libro, LibroMostrarEnAutoresDTO>();


            // COMENTARIOS
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
            CreateMap<Comentario, ComentarioUsuariosLogueadosDTO>();
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            // La lista de AutoresLibros que voy a devolver
            var resultado = new List<AutorLibro>();

            // Si el libro que se esta creando no tiene autores
            if (libroCreacionDTO.AutoresIds == null)
                return resultado;

            foreach (var id in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro { AutorId = id });
            }


            return resultado;
        }

        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            // La lista de AutorDto que voy a devolver
            var resultado = new List<AutorDTO>();

            // Si el libro que se esta creando no tiene autores
            if (libro.AutoresLibros is null)
                return resultado;

            foreach(var autorLibro in libro.AutoresLibros)
            {
                // Creo y agrego un nuevo AutorDTO, sacando los datos desde cada autorLibro en AutoresLibros 
                resultado.Add(new AutorDTO { Id = autorLibro.AutorId, 
                    Apellido = autorLibro.Autor.Apellido, 
                    Nombre = autorLibro.Autor.Nombre,
                    Descripcion = autorLibro.Autor.Descripcion,
                    FechaNacimiento = autorLibro.Autor.FechaNacimiento,
                    SourceFoto = autorLibro.Autor.SourceFoto,
                    SourceBiografia = autorLibro.Autor.SourceBiografia
                });
            }

            return resultado;
        }

        private List<LibroMostrarEnAutoresDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroMostrarEnAutoresDTO>();

            if (autor.AutoresLibros is null) // Si no tengo ningún libro
                return resultado;
            
            foreach(var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroMostrarEnAutoresDTO {
                    Id = autorLibro.LibroId, 
                    Titulo = autorLibro.Libro.Titulo,
                    Sinopsis = autorLibro.Libro.Sinopsis,
                    URLIden = autorLibro.Libro.URLIden,
                    SourcePortada = autorLibro.Libro.SourcePortada,
                    FechaPublicacion = autorLibro.Libro.FechaPublicacion
                });
            }

            return resultado;
        }
    }
}
