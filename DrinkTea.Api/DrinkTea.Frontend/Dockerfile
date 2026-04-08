FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# НИЧЕГО НЕ ПЕРЕИМЕНОВЫВАЕМ. 
# Просто создаем ОДНУ ссылку для главного файла, который ищет index.html
RUN cd _framework && \
    gunzip -k *.gz || true && \
    # Создаем копию/ссылку: blazor.webassembly.js -> blazor.webassembly.хэш.js
    REAL_FILE=$(ls blazor.webassembly.*.js | head -n 1) && \
    cp "$REAL_FILE" blazor.webassembly.js

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
