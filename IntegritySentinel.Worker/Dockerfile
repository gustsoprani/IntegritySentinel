# Estágio 1: Build (Compilação)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntegritySentinel.Worker.csproj", "./"]
RUN dotnet restore "IntegritySentinel.Worker.csproj"
COPY . .
RUN dotnet publish "IntegritySentinel.Worker.csproj" -c Release -o /app/publish

# Estágio 2: Runtime (Execução Leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IntegritySentinel.Worker.dll"]