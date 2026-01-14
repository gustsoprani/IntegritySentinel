# Estágio de Base (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Cria os diretórios que o app vai usar e dá permissão para o usuário 'app'
# O usuário 'app' tem UID 1654 nas imagens Microsoft
USER root
RUN mkdir -p /app/logs /app/data /app/monitorada \
    && chown -R app:app /app/logs /app/data /app/monitorada

# Define que, daqui para baixo, quem roda é o usuário 'app' (não mais o root)
USER app

# Estágio de Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IntegritySentinel.Worker.csproj", "."]
RUN dotnet restore "./IntegritySentinel.Worker.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./IntegritySentinel.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de Publicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IntegritySentinel.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Garante permissões finais (caso o copy tenha alterado algo)
USER root
RUN chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "IntegritySentinel.Worker.dll"]