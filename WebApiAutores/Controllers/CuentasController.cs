using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
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
            return usuarioConPassword.Succeeded ? await ConstruirToken(credencialesUsuario) : BadRequest(usuarioConPassword.Errors);

        }

        /// <summary>
        /// Loguearse en la aplicación
        /// </summary>
        /// <param name="credencialesUsuario">Email y Password del usuario</param>
        /// <returns>Token (JWT) y fecha de expiración del mismo</returns>
        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            // ME LOGUEO
            var logueoUsuario = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            return logueoUsuario.Succeeded ? await ConstruirToken(credencialesUsuario) : BadRequest("Login incorrecto");

        }

        /// <summary>
        /// Hacer admin a un usuario
        /// </summary>
        /// <param name="editarAdminDTO">Email del usuario</param>
        /// <returns></returns>
        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            // Traigo el usuario que tenga ese Email
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            // userManagaer no me trae el username automaticamente, por lo que lo asigno (el username es el email)
            usuario.UserName = usuario.Email;

            // Agrego el claim al usuario
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1")); // El valor del claim puede ser cualquier cosa
            return NoContent();

        }

        /// <summary>
        /// Quitar el Admin a un usuario
        /// </summary>
        /// <param name="editarAdminDTO">Email del usuario</param>
        /// <returns></returns>
        [HttpPost("RemoverAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            usuario.UserName = usuario.Email;

            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        /// <summary>
        /// Este método permite construir una nueva RespuestaAutenticacion con su JWT(Token) y fecha de expiración
        /// </summary>
        /// <param name="credencialesUsuario">Email y password</param>
        /// <returns>Token y fecha de expiración</returns>
        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // Construyo un listado de claims, es decir, información del usuario en la que confiamos(Información no importante)
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email)
            };

            // Traigo el usuario con ese email para poder agregar los claims que ya tenia (en caso de que se admin)
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            // Traigo los claims que ya tenia el usuario
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            // Agrego a los claims que cree los claims que ya tenia
            claims.AddRange(claimsDB);

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
