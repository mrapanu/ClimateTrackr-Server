global using Microsoft.EntityFrameworkCore;
global using ClimateTrackr_Server.Models;
global using ClimateTrackr_Server.Data;
global using ClimateTrackr_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;


//DotNetEnv.Env.Load($"{Directory.GetCurrentDirectory()}\\.env");
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

//RabbitMQ Service

builder.Services.Configure<RabbitMQConfig>(o =>
{
    o.ConnectionUrl = Environment.GetEnvironmentVariable("RABBITMQ_CONN_STRING")!;
    o.RoutingKey = Environment.GetEnvironmentVariable("RABBITMQ_ROUTING_KEY")!;
    o.ExchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_NAME")!;
});
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();
builder.Services.AddHostedService<ConsumerHostedService>();

//Report Service

builder.Services.AddHostedService<ReportService>();

//Notification Service

builder.Services.AddHostedService<NotificationService>();

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
            logger.LogInformation("Database created successfully.");
            // Create default admin user
            var adminUser = new User
            {
                Username = "ctadmin",
                PasswordHash = Convert.FromHexString("10E9D598D32B9966015729EFF8F2EB4743DC8907B75F9281F6775EBE368F17D04A2C0DB091E40A926F238D25309FE9FAAC71353A662D5C3BE573051242801A33"),
                PasswordSalt = Convert.FromHexString("59C5D8E6CAD208D3AC3B80A40E7C979ACE4233101D8C89E0C7D03EB54551A5121A8358B0D497DBF38882A2C9D03D6D551D6A8CB4D77CCEDA91ACAD17B780AC2A747E26DE598A261FB403C7E6EE1D4230644398C5C4883EA14C04427D9D26340092112BDE02820688A7CE4346BF368FA2D99927FD125A57D5836E36D0B4B1B473"),
                Usertype = UserType.Admin
            };
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

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
