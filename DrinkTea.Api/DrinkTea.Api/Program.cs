using DrinkTea.BL.Services;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Инфраструктура
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. DataAccess Layer
// Передаем строку подключения из appsettings.json в фабрику
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<ITeaRepository, TeaRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IBrewingRepository, BrewingRepository>();

// 3. Business Logic Layer
builder.Services.AddScoped<BrewingService>();
builder.Services.AddScoped<VisitService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
