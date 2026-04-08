# Сборка
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DrinkTea.Api/DrinkTea.Api.csproj", "DrinkTea.Api/"]
COPY ["DrinkTea.DataAccess/DrinkTea.DataAccess.csproj", "DrinkTea.DataAccess/"]
COPY ["DrinkTea.BL/DrinkTea.BL.csproj", "DrinkTea.BL/"]
COPY ["DrinkTea.Domain/DrinkTea.Domain.csproj", "DrinkTea.Domain/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]

RUN dotnet restore "DrinkTea.Api/DrinkTea.Api.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Api/DrinkTea.Api.csproj" -c Release -o /app/publish

# Этап запуска
FROM ://microsoft.com
WORKDIR /app

# Устанавливаем библиотеку для Kerberos (нужна для Npgsql)
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Api.dll"]
