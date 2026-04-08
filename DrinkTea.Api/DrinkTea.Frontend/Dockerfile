FROM ://microsoft.com AS build
WORKDIR /src
COPY . .
RUN dotnet publish "DrinkTea.Frontend/DrinkTea.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# Универсальное переименование: ищем файлы с двумя точками (name.hash.ext) 
# и переименовываем в name.ext, а также правим манифест blazor.boot.json
RUN cd _framework && \
    for f in *.*.js; do mv "$f" "${f%%.*}.js" 2>/dev/null || true; done && \
    for f in *.*.wasm; do mv "$f" "${f%%.*}.wasm" 2>/dev/null || true; done && \
    if [ -f blazor.boot.json ]; then \
        sed -i -E 's/\.[a-z0-9]{10}\.(js|wasm)/.\2/g' blazor.boot.json; \
    fi

COPY DrinkTea.Frontend/nginx.conf /etc/nginx/nginx.conf
