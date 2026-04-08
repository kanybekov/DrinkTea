# Сборка — нужен SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DrinkTea.Migrator/DrinkTea.Migrator.csproj", "DrinkTea.Migrator/"]
# Если мигратор использует Domain или DataAccess, добавь их COPY сюда
RUN dotnet restore "DrinkTea.Migrator/DrinkTea.Migrator.csproj"

COPY . .
RUN dotnet publish "DrinkTea.Migrator/DrinkTea.Migrator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Migrator.dll"]
