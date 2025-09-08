# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY *.sln .
COPY GenstarXKulayInventorySystem.Server/*.csproj ./GenstarXKulayInventorySystem.Server/
COPY GenstarXKulayInventorySystem.Client/*.csproj ./GenstarXKulayInventorySystem.Client/
COPY GenstarXKulayInventorySystem.Shared/*.csproj ./GenstarXKulayInventorySystem.Shared/

RUN dotnet restore

# Copy everything else
COPY . .

# Publish Server (includes Client static files)
RUN dotnet publish GenstarXKulayInventorySystem.Server -c Release -o /app/publish

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose Railway port
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "GenstarXKulayInventorySystem.Server.dll"]
