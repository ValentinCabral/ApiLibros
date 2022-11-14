using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class ComentarioDTO
    {
        public int Id { get; set; }

        [Required]
        public string Contenido { get; set; }
    }
}
