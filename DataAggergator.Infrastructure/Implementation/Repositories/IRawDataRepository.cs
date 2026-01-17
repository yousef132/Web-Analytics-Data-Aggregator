using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Infrastructure.Implementation.Repositories
{
    internal class RawDataRepository(AnalyticsDbContext _context) : IRawDataRepository
    {
        public async Task<List<RawData>?> GetPageOverView(CancellationToken cancellationToken)
        {
            return await _context.RawData.GroupBy(r=> r.Page)
                                             .Select(g=> new RawData
                                             {
                                                 Page = g.Key,
                                                 Users = g.Sum(x=> x.Users),
                                                 Sessions = g.Sum(x=> x.Sessions),
                                                 Views = g.Sum(x=> x.Views),
                                                 PerformanceScore = g.Average(x=> x.PerformanceScore),
                                             }).ToListAsync(cancellationToken);
        }
    }
}
