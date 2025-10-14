# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore
COPY HelloCountryService.csproj ./
COPY data data
RUN dotnet restore HelloCountryService.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish HelloCountryService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV DOTNET_EnableDiagnostics=0 \
    PORT=8080

COPY --from=build /app/publish .

EXPOSE 8080

CMD ["dotnet", "HelloCountryService.dll"]
