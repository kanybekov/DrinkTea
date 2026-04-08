FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# ПРИНУДИТЕЛЬНОЕ ПРИВЕДЕНИЕ К СТАНДАРТУ
RUN cd _framework && \
    # 1. Распаковываем (Nginx на 1ГБ RAM не всегда дружит с brotli/gzip без модулей)
    gunzip -k *.gz || true && \
    # 2. Убираем хеши из всех .js и .wasm (находим файлы с 2+ точками и сокращаем)
    for f in $(ls | grep -E '[a-z0-9]{10}\.(js|wasm)$'); do \
        mv "$f" "$(echo $f | sed -E 's/\.[a-z0-9]{10}\././')" ; \
    done && \
    # 3. Находим и переименовываем манифест (boot.json.хеш -> boot.json)
    find . -name "blazor.boot.json*" -exec mv {} blazor.boot.json \; 2>/dev/null || true && \
    # 4. Исправляем ссылки внутри манифеста
    if [ -f blazor.boot.json ]; then sed -i -E 's/\.[a-z0-9]{10}\././g' blazor.boot.json; fi

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
