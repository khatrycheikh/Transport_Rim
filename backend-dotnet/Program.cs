using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TransportRim.Api.Data;
using TransportRim.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration des Contrôleurs et Sérialisation JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure la sérialisation des énumérations sous forme de chaînes de caractères (ex: "Admin", "Masrivi") dans le JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configuration de Swagger / OpenAPI avec prise en charge JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Transport Rim API",
        Version = "v1",
        Description = "API de Gestion du Transport Interurbain en Mauritanie"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer' suivi d'un espace et de votre token JWT.\n\nExemple: \"Bearer eyJhbGciOiJIUzI1Ni...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            System.Array.Empty<string>()
        }
    });
});

// 2. Configuration du DbContext MySQL (Entity Framework Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TransportRimDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 3. Configuration de l'Authentification JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] 
    ?? throw new InvalidOperationException("La clé secrète JWT n'est pas configurée dans appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Pour invalider le token instantanément à sa date de péremption
    };
});

// 4. Injection des Dépendances (DI) pour les Services Métier
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IBusService, BusService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// 5. Configuration CORS pour l'application Angular (tournant sur http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Permet la transmission d'en-têtes et de cookies sécurisés
    });
});

var app = builder.Build();

// Seed automatique de la base de données
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TransportRimDbContext>();
        await TransportRim.Api.Data.DataSeeder.SeedAsync(context);

        // Affichage des informations de diagnostic sur le serveur et la base de données
        var conn = context.Database.GetDbConnection();
        var companiesCount = await context.Companies.CountAsync();
        var busesCount = await context.Buses.CountAsync();
        var tripsCount = await context.Trips.CountAsync();
        
        Console.WriteLine("==================================================");
        Console.WriteLine("[DIAGNOSTIC BACKEND]");
        Console.WriteLine($"Serveur connecté  : {conn.DataSource}");
        Console.WriteLine($"Base de données   : {conn.Database}");
        Console.WriteLine($"Compagnies en base: {companiesCount}");
        Console.WriteLine($"Bus en base       : {busesCount}");
        Console.WriteLine($"Trajets en base   : {tripsCount}");
        Console.WriteLine("==================================================");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur est survenue lors de l'initialisation des données (Seed).");
    }
}

// Activation de Swagger pour le développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transport Rim API v1");
    });
}

app.UseHttpsRedirection();

// Activation du CORS (doit précéder l'authentification dans le pipeline)
app.UseCors("AngularCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
