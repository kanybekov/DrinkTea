FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проекты для восстановления зависимостей
COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]
RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"

# Копируем всё остальное и публикуем
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

# Этап раздачи (Nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Копируем статику
COPY --from=build /app/publish/wwwroot .

# Фикс хешей и распаковка для Nginx
RUN cd _framework && \
    gunzip -k *.gz || true && \
    # Создаем входные точки без хешей для index.html и рантайма
    cp blazor.webassembly.*.js blazor.webassembly.js 2>/dev/null || true && \
    DOTNET_MAIN_JS=$(ls dotnet.*.js | grep -v "runtime" | grep -v "native" | head -n 1) && \
    cp "$DOTNET_MAIN_JS" dotnet.js 2>/dev/null || true

# Копируем кастомный конфиг Nginx (с прокси на /api)
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
