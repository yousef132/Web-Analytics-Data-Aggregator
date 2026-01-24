using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Domain.Models;

namespace DataAggergator.Application.Abstractions.Reposioties
{
    public interface IRawDataRepository
    {
        Task<List<RawData>?> GetPageOverView(CancellationToken cancellationToken);

    }

}
