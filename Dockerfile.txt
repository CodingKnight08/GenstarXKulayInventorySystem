# Stage 1: Build with .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY *.sln .
COPY GenstarXKulayInventorySystem.Server/*.csproj GenstarXKulayInventorySystem.Server/
COPY GenstarXKulayInventorySystem.Client/*.csproj GenstarXKulayInventorySystem.Client/
COPY GenstarXKulayInventorySystem.Shared/*.csproj GenstarXKulayInventorySystem.Shared/

# Restore dependencies
RUN dotnet restore

# Copy the full source
COPY . .

# Publish the Server project (includes Client if referenced)
WORKDIR /src/GenstarXKulayInventorySystem.Server
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime with ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Railway injects PORT, so bind to it
ENV ASPNETCORE_URLS=http://+:$PORT

# Run the server (API + Blazor WASM hosting)
ENTRYPOINT ["dotnet", "GenstarXKulayInventorySystem.Server.dll"]
