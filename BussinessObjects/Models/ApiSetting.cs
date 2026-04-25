using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.Models
{
    public class ApiSetting
    {
        public Guid Id { get; set; }

        public string? GeminiApiKey { get; set; }

        public string? ColabApiUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
