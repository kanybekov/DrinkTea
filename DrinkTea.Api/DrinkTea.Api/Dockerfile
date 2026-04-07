# Этап сборки (используем SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем все файлы проектов для восстановления зависимостей
COPY ["DrinkTea.Api/DrinkTea.Api.csproj", "DrinkTea.Api/"]
COPY ["DrinkTea.Domain/DrinkTea.Domain.csproj", "DrinkTea.Domain/"]
COPY ["DrinkTea.BL/DrinkTea.BL.csproj", "DrinkTea.BL/"]
COPY ["DrinkTea.DataAccess/DrinkTea.DataAccess.csproj", "DrinkTea.DataAccess/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

RUN dotnet restore "DrinkTea.Api/DrinkTea.Api.csproj"

# Копируем остальной код и публикуем
COPY . .
RUN dotnet publish "DrinkTea.Api/DrinkTea.Api.csproj" -c Release -o /app/publish

# Финальный этап (используем Runtime — он легче)
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Api.dll"]
