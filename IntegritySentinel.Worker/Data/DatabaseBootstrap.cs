using Dapper;
using IntegritySentinel.Worker.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace IntegritySentinel.Worker.Data;

public class DatabaseBootstrap
{
    private readonly MonitorSettings _settings;
    private readonly string _connectionString;

    // Recebemos as configurações via Injeção de Dependência
    public DatabaseBootstrap(IConfiguration configuration)
    {
        // Pegamos a connection string do appsettings.json
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void Setup()
    {
        using var connection = new SqliteConnection(_connectionString);

        // Aqui entra o TEU SQL (levemente ajustado para Dapper)
        var sql = @"
            CREATE TABLE IF NOT EXISTS Files (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FilePath TEXT NOT NULL,
                Hash TEXT,
                LastModified TEXT
            );
            
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Files_FilePath ON Files(FilePath);
        ";

        // O Dapper executa o comando
        connection.Execute(sql);
    }
}