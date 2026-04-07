# 1. Используем SDK для сборки (Microsoft Container Registry)
FROM mcr.microsoft.com AS build
WORKDIR /src

# Копируем проект и восстанавливаем зависимости
COPY ["DrinkTea.Migrator/DrinkTea.Migrator.csproj", "DrinkTea.Migrator/"]
RUN dotnet restore "DrinkTea.Migrator/DrinkTea.Migrator.csproj"

# Копируем остальной код и публикуем
COPY . .
RUN dotnet publish "DrinkTea.Migrator/DrinkTea.Migrator.csproj" -c Release -o /app/publish

# 2. Используем Runtime для запуска (он намного меньше по размеру)
FROM mcr.microsoft.com
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Migrator.dll"]
