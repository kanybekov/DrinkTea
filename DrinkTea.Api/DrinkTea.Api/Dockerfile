# Сборка
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Все пути относительно папки, где лежит docker-compose.yml
COPY ["DrinkTea.Api/DrinkTea.Api.csproj", "DrinkTea.Api/"]
COPY ["DrinkTea.Domain/DrinkTea.Domain.csproj", "DrinkTea.Domain/"]
COPY ["DrinkTea.BL/DrinkTea.BL.csproj", "DrinkTea.BL/"]
COPY ["DrinkTea.DataAccess/DrinkTea.DataAccess.csproj", "DrinkTea.DataAccess/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

RUN dotnet restore "DrinkTea.Api/DrinkTea.Api.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Api/DrinkTea.Api.csproj" -c Release -o /app/publish

# Финал (Runtime для экономии RAM)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Api.dll"]
