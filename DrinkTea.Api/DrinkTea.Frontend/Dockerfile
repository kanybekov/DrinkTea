FROM ://microsoft.com AS build
WORKDIR /src
COPY ["DrinkTea.Frontend/DrinkTea.Frontend.csproj", "DrinkTea.Frontend/"]
COPY ["DrinkTea.Shared/DrinkTea.Shared.csproj", "DrinkTea.Shared/"]
RUN dotnet restore "DrinkTea.Frontend/DrinkTea.Frontend.csproj"
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
