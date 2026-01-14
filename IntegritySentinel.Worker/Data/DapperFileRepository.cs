using IntegritySentinel.Worker.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using IntegritySentinel.Worker.Domain.Entities;

namespace IntegritySentinel.Worker.Data
{
    public class DapperFileRepository : IFileRepository
    {
        private readonly string _connectionString;
        public DapperFileRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task Add(FileRecord file)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"INSERT INTO Files (FilePath, Hash, LastModified) VALUES (@FilePath, @Hash, @LastModified)";
            await connection.ExecuteAsync(sql, file);
        }

        public async Task Update(FileRecord file)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"UPDATE Files SET Hash = @Hash, LastModified = @LastModified WHERE Id = @Id";
            await connection.ExecuteAsync(sql, file); 
        }

        public async Task<FileRecord?> Search(string path)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"SELECT * FROM Files WHERE FilePath = @FilePath LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<FileRecord?>(sql, new { FilePath = path });
        }

        public async Task<IEnumerable<FileRecord>> SearchAll()
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"SELECT * FROM Files";
            return await connection.QueryAsync<FileRecord>(sql);
        }

        public async Task Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "DELETE FROM Files WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
