FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

# Собираем с ПРИНУДИТЕЛЬНЫМ отключением всех новых фишек .NET 10
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish \
    /p:StaticWebAssetFingerprintingEnabled=false \
    /p:BlazorEnableCompression=false

FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Копируем чистую статику (теперь она будет БЕЗ хешей и БЕЗ сжатия)
COPY --from=build /app/publish/wwwroot .

# На всякий случай проверяем, не спрятал ли он манифест под другим именем
RUN cd _framework && \
    if [ -f blazor.boot.json.* ]; then mv blazor.boot.json.* blazor.boot.json; fi

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
