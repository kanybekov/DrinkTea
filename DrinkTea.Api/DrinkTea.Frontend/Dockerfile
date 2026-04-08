FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
# Копируем только статику Blazor
COPY --from=build /app/publish/wwwroot .
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
