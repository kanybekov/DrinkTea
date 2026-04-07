FROM ://microsoft.com AS build
WORKDIR /src
COPY ["DrinkTea.Migrator/DrinkTea.Migrator.csproj", "DrinkTea.Migrator/"]
RUN dotnet restore "DrinkTea.Migrator/DrinkTea.Migrator.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Migrator/DrinkTea.Migrator.csproj" -c Release -o /app/publish

FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrinkTea.Migrator.dll"]