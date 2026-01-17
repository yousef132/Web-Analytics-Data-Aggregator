using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Application.Dtos;

namespace DataAggergator.Domain.Messages
{
    public record AnalyticsRecordsAggregated
    {
        public AnalyticsRecordsAggregated(List<AnalyticsRecord> records)
        {
            Records = records;
        }
        public List<AnalyticsRecord> Records { get; set; }
    }
}
