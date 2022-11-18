using Microsoft.AspNetCore.Identity;

namespace WebApiAutores.Entidades
{
    public class Biblioteca
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }
        public List<Libro> Libros { get; set; }
    }
}
