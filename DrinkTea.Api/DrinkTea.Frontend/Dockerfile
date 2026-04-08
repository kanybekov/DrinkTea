FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
# Собираем как есть, со всеми хешами (пусть будут)
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
# Копируем всё содержимое publish/wwwroot
COPY --from=build /app/publish/wwwroot .

# КЛЮЧЕВОЙ ШАГ: Распаковываем все .gz файлы. 
# Это создаст обычные .js и .wasm рядом со сжатыми.
# Nginx увидит их и сможет отдать браузеру.
RUN gunzip -k _framework/*.gz || true

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
