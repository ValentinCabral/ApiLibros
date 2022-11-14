using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroMostrarEnAutoresDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        public string Titulo { get; set; }
        public string? Sinopsis { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string URLIden { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string SourcePortada { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
