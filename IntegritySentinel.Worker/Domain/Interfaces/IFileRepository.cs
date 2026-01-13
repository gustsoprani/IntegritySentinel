using IntegritySentinel.Worker.Domain.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegritySentinel.Worker.Domain.Interfaces
{
    public interface IFileRepository
    {
        Task<FileRecord?> Search(string path);
        Task Add(FileRecord file);
        Task Update(FileRecord file);
        Task<IEnumerable<FileRecord>> SearchAll();
    }
}
