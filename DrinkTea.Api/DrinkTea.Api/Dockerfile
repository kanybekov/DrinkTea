FROM ://microsoft.com AS build
WORKDIR /src
# Копируем все зависимости
COPY ["DrinkTea.Api/DrinkTea.Api.csproj", "DrinkTea.Api/"]
COPY ["DrinkTea.Domain/DrinkTea.Domain.csproj", "DrinkTea.Domain/"]
COPY ["DrinkTea.BL/DrinkTea.BL.csproj", "DrinkTea.BL/"]
COPY ["DrinkTea.DataAccess/DrinkTea.DataAccess.csproj", "DrinkTea.DataAccess/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]
RUN dotnet restore "DrinkTea.Api/DrinkTea.Api.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Api/DrinkTea.Api.csproj" -c Release -o /app/publish

FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Api.dll"]