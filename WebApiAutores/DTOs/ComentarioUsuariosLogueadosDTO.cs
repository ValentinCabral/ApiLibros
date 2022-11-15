using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class ComentarioUsuariosLogueadosDTO
    {
        public int Id { get; set; }

        [Required]
        public string Contenido { get; set; }
    }
}
