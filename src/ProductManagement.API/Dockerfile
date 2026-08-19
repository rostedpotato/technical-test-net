# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/ProductManagement.Core/ProductManagement.Core.csproj", "src/ProductManagement.Core/"]
COPY ["src/ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj", "src/ProductManagement.Infrastructure/"]
COPY ["src/ProductManagement.API/ProductManagement.API.csproj", "src/ProductManagement.API/"]
RUN dotnet restore "src/ProductManagement.API/ProductManagement.API.csproj"

# Copy full source and build
COPY ["src/ProductManagement.Core/", "src/ProductManagement.Core/"]
COPY ["src/ProductManagement.Infrastructure/", "src/ProductManagement.Infrastructure/"]
COPY ["src/ProductManagement.API/", "src/ProductManagement.API/"]
WORKDIR "/src/src/ProductManagement.API"
RUN dotnet build "ProductManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5187
ENV ASPNETCORE_URLS=http://+:5187
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "ProductManagement.API.dll"]
