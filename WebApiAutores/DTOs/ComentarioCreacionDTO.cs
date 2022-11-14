using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class ComentarioCreacionDTO
    {
        [Required]
        public string Contenido { get; set; }
    }
}
