FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# СОЗДАЕМ ССЫЛКИ ДЛЯ ЗАГРУЗЧИКОВ
RUN cd _framework && \
    gunzip -k *.gz || true && \
    # Ссылка для основного скрипта
    REAL_BLAZOR=$(ls blazor.webassembly.*.js | head -n 1) && \
    ln -sf "$REAL_BLAZOR" blazor.webassembly.js && \
    # Ссылка для dotnet.js (его тоже ищут без хеша)
    REAL_DOTNET=$(ls dotnet.*.js | grep -v "native" | grep -v "runtime" | head -n 1) && \
    ln -sf "$REAL_DOTNET" dotnet.js

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
