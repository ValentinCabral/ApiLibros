using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // Para configurar la llave primaria de AutoresLibros
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AutorLibro>().HasKey(autorLibro => new {autorLibro.AutorId, autorLibro.LibroId });
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LibroBibliotecas>().HasKey(LibroBibliotecas => new { LibroBibliotecas.LibroId, LibroBibliotecas.BibliotecaId });
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }
        public DbSet<Biblioteca> Bibliotecas { get; set; }
        public DbSet<LibroBibliotecas> LibroBibliotecas { get; set; }
    }
}
