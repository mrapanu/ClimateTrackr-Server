global using Microsoft.EntityFrameworkCore;
global using ClimateTrackr_Server.Models;
global using ClimateTrackr_Server.Data;
global using ClimateTrackr_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;


DotNetEnv.Env.Load($"{Directory.GetCurrentDirectory()}\\.env");
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(o =>
{
    o.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONN_STRING"));
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = """Standard Authorization header using the Bearer scheme. Example: "bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.
            GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_TOKEN")!)),
            ValidateIssuer = false,
            ValidateAudience = false   //TO BE REVISED.
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//RabbitMQ

builder.Services.Configure<RabbitMQConfig>(o =>
{
    o.ConnectionUrl = Environment.GetEnvironmentVariable("RABBITMQ_CONN_STRING")!;
    o.RoutingKey = Environment.GetEnvironmentVariable("RABBITMQ_ROUTING_KEY")!;
    o.ExchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_NAME")!;
});
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();
builder.Services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = serviceScope.ServiceProvider.GetRequiredService<DataContext>().Database;
    if (!db.CanConnect())
    {
        try
        {
            db.EnsureCreated();
            db.Migrate();
            logger.LogInformation("Database created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the database.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.Run();
