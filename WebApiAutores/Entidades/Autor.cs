using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 50, ErrorMessage = ("El campo {0} debe contener como máximo {1} carácteres"))]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 50, ErrorMessage = ("El campo {0} debe contener como máximo {1} carácteres"))]
        public string Apellido { get; set; }
        public string? Descripcion { get; set; }
        [Url(ErrorMessage = "El campo {0} debe ser una URL}")]
        public DateTime FechaNacimiento { get; set; }
        public string SourceFoto { get; set; }
        [Url(ErrorMessage = "El campo {0} debe ser una URL}")]
        public string SourceBiografia { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }
    }
}
