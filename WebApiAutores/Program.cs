using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores;
using WebApiAutores.Servicios;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson(option =>
{
    option.SerializerSettings.DateFormatString = "yyyy";
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id =  "Bearer"
                },
            },
            new string[]{}
        }
    });

    // Para la documentación en comentarios
    var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
    c.IncludeXmlComments(rutaXML);
});
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IOrdenarAutores, OrdenarAutores>();
builder.Services.AddCors(opciones =>
{
    // Agregamos politica por defecto
    //opciones.AddDefaultPolicy(builder =>
    opciones.AddPolicy(name: MyAllowSpecificOrigins, builder =>
    {
        // Para las URL que van a tener acceso a la API
        builder
            .AllowAnyOrigin() // Permite notificaciones de cualquier origen
        //WithOrigins("")
            .AllowAnyMethod() // Permite cualquier metodo HTTP (POST, PUT, GET, etc)
            .AllowAnyHeader() // Permite cualquier cabecera
            .WithExposedHeaders(); // Exponer cabeceras que vamos a devolver desde la API
    });
});

/*
 PARA USAR IDENTITY
 */
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LlaveJWT"])),
        ClockSkew = TimeSpan.Zero

    });


// Limpio el mapeo que se hace en los tipos de los claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


// Configuro la autorizacion basada en claims para los administradores
builder.Services.AddAuthorization(opciones =>
{
    // Agrego la politica EsAdmin que requiere un claim llamado esAdmin
    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Activamos CORS
app.UseCors(MyAllowSpecificOrigins);

app.Run();
