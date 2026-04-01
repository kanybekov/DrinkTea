using DrinkTea.BL.Services;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.DataAccess.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Инфраструктура
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Превращает Enum в строку (и наоборот) при передаче по сети
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Это заставит Swagger отображать енамы как строки, если используется Swashbuckle
    c.DescribeAllParametersInCamelCase();
});

// 2. DataAccess Layer
// Передаем строку подключения из appsettings.json в фабрику
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<ITeaRepository, TeaRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IBrewingRepository, BrewingRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();

// 3. Business Logic Layer
builder.Services.AddScoped<BrewingService>();
builder.Services.AddScoped<VisitService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<TeaService>();

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
