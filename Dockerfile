# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar o csproj da subpasta
COPY authorizationDemo/*.csproj ./authorizationDemo/
WORKDIR /app/authorizationDemo

RUN dotnet restore

# Copiar o restante dos arquivos
COPY authorizationDemo/. ./

RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/authorizationDemo/publish .

ENTRYPOINT ["dotnet", "AuthorizationDemo.dll"]
