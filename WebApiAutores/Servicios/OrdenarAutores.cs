using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores.Servicios
{
    public interface IOrdenarAutores
    {
        Task<List<Autor>> TraerAutores(int ordenarPor,int tipoOrden);
    }
    public class OrdenarAutores : IOrdenarAutores
    {
        private readonly ApplicationDbContext context;

        public OrdenarAutores(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<List<Autor>> TraerAutores(int ordenarPor, int tipoOrden)
        {
            switch (tipoOrden)
            {
                case 1:
                    switch (ordenarPor)
                    {
                        case 1:
                            var autoresNombreAsc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderBy(x => x.Nombre)
                            .ThenBy(x => x.Apellido)
                            .ToListAsync();
                            return autoresNombreAsc;
                        case 2:
                            var autoresApellidoAsc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderBy(x => x.Apellido)
                            .ThenBy(x => x.Nombre)
                            .ToListAsync();
                            return autoresApellidoAsc;
                        case 3:
                            var autoresFechaAsc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderBy(x => x.FechaNacimiento)
                            .ThenBy(x => x.Id)
                            .ToListAsync();
                            return autoresFechaAsc;
                        default:
                            var autoresAsc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .ToListAsync();
                            return autoresAsc;

                    }
                default:
                    switch (ordenarPor)
                    {
                        case 1:
                            var autoresNombreDesc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderByDescending(x => x.Nombre)
                            .ThenByDescending(x => x.Apellido)
                            .ToListAsync();
                            return autoresNombreDesc;

                        case 2:
                            var autoresApellidoDesc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderByDescending(x => x.Apellido)
                            .ThenByDescending(x => x.Nombre)
                            .ToListAsync();
                            return autoresApellidoDesc;
                        case 3:
                            var autoresFechaDesc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderByDescending(x => x.FechaNacimiento)
                            .ThenByDescending(x => x.Id)
                            .ToListAsync();
                            return autoresFechaDesc;
                        default:
                            var autoresDesc = await context.Autores
                            .Include(x => x.AutoresLibros)
                            .ThenInclude(autorLibro => autorLibro.Libro)
                            .OrderByDescending(x => x.Id)
                            .ToListAsync();
                            return autoresDesc;

                    }
            }
    
        }
    }
}
