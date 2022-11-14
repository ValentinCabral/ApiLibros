using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public CuentasController(IConfiguration configuration, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// Este endpoint permite registar un nuevo usuario en la aplicación
        /// </summary>
        /// <param name="credencialesUsuario">Recibe como parametro un Email y un Password</param>
        /// <returns>Token (JWT) y fecha de expiración del nuevo usuario</returns>
        [HttpPost("registrar")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            // Creo el nuevo usuario sin su password
            var usuario = new IdentityUser
            {
                Email = credencialesUsuario.Email,
                UserName = credencialesUsuario.Email
            };

            // El resultado con el password
            var usuarioConPassword = await userManager.CreateAsync(usuario, credencialesUsuario.Password); // CreateAsync crea un nuevo usuario.

            // Si el usuario se creo con éxito
            return usuarioConPassword.Succeeded ? ConstruirToken(credencialesUsuario) : BadRequest(usuarioConPassword.Errors);

        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            // ME LOGUEO
            var logueoUsuario = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            return logueoUsuario.Succeeded ? ConstruirToken(credencialesUsuario) : BadRequest("Login incorrecto");

        }

        /// <summary>
        /// Este método permite construir una nueva RespuestaAutenticacion con su JWT(Token) y fecha de expiración
        /// </summary>
        /// <param name="credencialesUsuario">Email y password</param>
        /// <returns>Token y fecha de expiración</returns>
        private RespuestaAutenticacion ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // Construyo un listado de claims, es decir, información del usuario en la que confiamos(Información no importante)
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email)
            };

            // Traigo la llave desde appsettings
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));

            // Construyo las credenciales
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);


            var expiracion = DateTime.UtcNow.AddYears(1);

            // GENERO EL TOKEN
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            // Retorno la respuesta autenticacion
            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
    }
}
