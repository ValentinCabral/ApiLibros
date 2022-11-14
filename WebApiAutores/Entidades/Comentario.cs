﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required]
        public string Contenido { get; set; }
        public int LibroId { get; set; }
        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }
        public Libro Libro { get; set; }
    }
}
