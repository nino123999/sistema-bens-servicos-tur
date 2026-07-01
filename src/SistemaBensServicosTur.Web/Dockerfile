FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/SistemaBensServicosTur.Web/SistemaBensServicosTur.Web.csproj src/SistemaBensServicosTur.Web/
RUN dotnet restore src/SistemaBensServicosTur.Web/SistemaBensServicosTur.Web.csproj

COPY src/SistemaBensServicosTur.Web/ src/SistemaBensServicosTur.Web/
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
