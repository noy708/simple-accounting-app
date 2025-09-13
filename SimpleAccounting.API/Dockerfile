# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimpleAccounting.API.csproj", "."]
RUN dotnet restore "./SimpleAccounting.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SimpleAccounting.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleAccounting.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/accounting.db"

ENTRYPOINT ["dotnet", "SimpleAccounting.API.dll"]