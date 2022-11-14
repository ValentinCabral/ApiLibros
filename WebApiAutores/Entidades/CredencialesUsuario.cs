using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Entidades
{
    public class CredencialesUsuario
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
