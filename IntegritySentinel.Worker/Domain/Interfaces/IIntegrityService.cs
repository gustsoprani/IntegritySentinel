using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegritySentinel.Worker.Domain.Interfaces
{
    public interface IIntegrityService
    {
        Task ExecuteCycleAsync(string targetPath, CancellationToken token);
    }
}
