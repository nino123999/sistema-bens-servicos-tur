FROM node:20-alpine AS css-builder
WORKDIR /app
COPY src/SistemaBensServicosTur.Web/package.json .
RUN npm install
COPY src/SistemaBensServicosTur.Web/tailwind.config.js .
COPY src/SistemaBensServicosTur.Web/wwwroot ./wwwroot/
COPY src/SistemaBensServicosTur.Web/Components ./Components/
RUN node_modules/.bin/tailwindcss -i ./wwwroot/input.css -o ./wwwroot/dist.css --minify

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/SistemaBensServicosTur.Web/SistemaBensServicosTur.Web.csproj src/SistemaBensServicosTur.Web/
RUN dotnet restore src/SistemaBensServicosTur.Web/SistemaBensServicosTur.Web.csproj

COPY src/SistemaBensServicosTur.Web/ src/SistemaBensServicosTur.Web/
COPY --from=css-builder /app/wwwroot/dist.css src/SistemaBensServicosTur.Web/wwwroot/dist.css

RUN dotnet publish src/SistemaBensServicosTur.Web/SistemaBensServicosTur.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p wwwroot/uploads

EXPOSE 8080

ENTRYPOINT ["dotnet", "SistemaBensServicosTur.Web.dll"]
