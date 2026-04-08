FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# Автоматизируем всё: распаковку и создание входных точек без хешей
RUN cd _framework && \
    gunzip -k *.gz || true && \
    # Создаем копии для тех файлов, которые index.html ищет без хеша
    cp blazor.webassembly.*.js blazor.webassembly.js 2>/dev/null || true && \
    cp dotnet.*.js dotnet.js 2>/dev/null || true

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
