FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Копируем статику
COPY --from=build /app/publish/wwwroot .

# МАГИЯ: Удаляем хеши из манифеста загрузки, если они там остались
RUN if [ -f _framework/blazor.boot.json ]; then \
    sed -i 's/\.[a-zA-Z0-9]\{10\}\.js/.js/g' _framework/blazor.boot.json && \
    sed -i 's/\.[a-zA-Z0-9]\{10\}\.wasm/.wasm/g' _framework/blazor.boot.json; \
    fi

# Копируем конфиг Nginx
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
