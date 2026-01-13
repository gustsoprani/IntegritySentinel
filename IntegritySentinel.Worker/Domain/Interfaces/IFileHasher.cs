using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegritySentinel.Worker.Domain.Interfaces
{
    public interface IFileHasher
    {
        Task<string?> HashAsync(string filePath);
    }
}
