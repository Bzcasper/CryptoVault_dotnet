# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["CryptoVault.csproj", "./"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set the environment variable to ensure it listens correctly
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CryptoVault.dll"]
