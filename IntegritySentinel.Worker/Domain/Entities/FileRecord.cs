using System;

namespace IntegritySentinel.Worker.Domain.Entities;

public class FileRecord
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string LastModified { get; set; } = string.Empty;
}