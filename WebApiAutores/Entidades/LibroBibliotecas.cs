namespace WebApiAutores.Entidades
{
    public class LibroBibliotecas
    {
        public int LibroId { get; set; }
        public int BibliotecaId { get; set; }
        public Libro Libro { get; set; }
        public Biblioteca Biblioteca { get; set; }
    }
}
