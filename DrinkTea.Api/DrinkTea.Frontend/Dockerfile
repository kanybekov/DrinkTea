# Исправлено: указан полный путь к SDK без ://
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проекты
COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

# Восстанавливаем зависимости
RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"

# Копируем всё и публикуем (генерируем статику wwwroot)
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

# Финальный этап на Nginx
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
