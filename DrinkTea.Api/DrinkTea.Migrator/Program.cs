using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

string connectionString = config.GetConnectionString("DefaultConnection");

// Ждем базу (Postgres может стартовать дольше контейнера)
EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithVariablesDisabled()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
return result.Successful ? 0 : -1;
