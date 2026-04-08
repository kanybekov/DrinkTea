FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
# Добавляем флаг отключения хешей прямо в команду сборки
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish /p:StaticWebAssetFingerprintingEnabled=false

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# СКРИПТ-ГАРПУН: Если файлы всё равно создались с хешами, этот код их переименует в "чистые" имена
RUN cd _framework && \
    for f in *.*.js; do mv "$f" "${f%%.*}.js" 2>/dev/null || true; done && \
    for f in *.*.wasm; do mv "$f" "${f%%.*}.wasm" 2>/dev/null || true; done && \
    if [ -f blazor.boot.json ]; then \
        sed -i -E 's/(\w+)\.[a-z0-9]{10}\.(js|wasm)/\1.\2/g' blazor.boot.json; \
    fi

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
