FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проекты (пути относительно DrinkTea.Api, где лежит docker-compose)
COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

# Восстанавливаем зависимости
RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"

# Копируем весь исходный код и публикуем
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

# 2. Этап раздачи (Nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Копируем статику из этапа сборки
COPY --from=build /app/publish/wwwroot .

# Универсальный фикс хешей и сжатия .NET 10
RUN cd _framework && \
    # Распаковываем всё, чтобы Nginx видел чистые файлы
    gunzip -k *.gz || true && \
    # Копируем основной загрузчик blazor
    cp blazor.webassembly.*.js blazor.webassembly.js 2>/dev/null || true && \
    # Динамически находим и копируем основной dotnet.js (исключая runtime и native)
    DOTNET_MAIN_JS=$(ls dotnet.*.js | grep -v "runtime" | grep -v "native" | head -n 1) && \
    cp "$DOTNET_MAIN_JS" dotnet.js 2>/dev/null || true

# Копируем конфиг Nginx
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
