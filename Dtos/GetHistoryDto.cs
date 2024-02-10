using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClimateTrackr_Server.Dtos
{
    public class GetHistoryDto
    {
        public string ActionMessage { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string User { get; set; } = string.Empty;
    }
}