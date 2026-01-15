# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/FoodDelivery.API/FoodDelivery.API.csproj", "src/FoodDelivery.API/"]
COPY ["src/FoodDelivery.Application/FoodDelivery.Application.csproj", "src/FoodDelivery.Application/"]
COPY ["src/FoodDelivery.Domain/FoodDelivery.Domain.csproj", "src/FoodDelivery.Domain/"]
COPY ["src/FoodDelivery.Infrastructure/FoodDelivery.Infrastructure.csproj", "src/FoodDelivery.Infrastructure/"]

RUN dotnet restore "src/FoodDelivery.API/FoodDelivery.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/FoodDelivery.API"
RUN dotnet build "FoodDelivery.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FoodDelivery.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodDelivery.API.dll"]
