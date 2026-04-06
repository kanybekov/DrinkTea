using DrinkTea.BL.Infrastructure;
using DrinkTea.BL.Services;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Инфраструктура
builder.Services.AddControllers()
    .AddJsonOptions(options => {});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DrinkTea API", Version = "v1" });

    // 1. Создаем схему (в 2.0.0 это делается так)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введите JWT токен",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // 2. Добавляем требование (в новой версии синтаксис чуть строже)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
        {
            policy.AllowAnyOrigin() // Разрешаем любой домен
                  .AllowAnyMethod() // Разрешаем GET, POST, PUT, DELETE, PATCH
                  .AllowAnyHeader(); // Разрешаем любые заголовки (включая Authorization)
        });
    });

builder.Services.AddSingleton<JwtProvider>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// 2. DataAccess Layer
// Передаем строку подключения из appsettings.json в фабрику
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<ITeaRepository, TeaRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IBrewingRepository, BrewingRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3. Business Logic Layer
builder.Services.AddScoped<BrewingService>();
builder.Services.AddScoped<VisitService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<TeaService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContext>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("DevCors");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

app.Run();
