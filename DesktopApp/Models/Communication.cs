using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class Communication
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        public string Type { get; set; } // email, phone, manual_note
        public string Content { get; set; }
        public string Direction { get; set; }
        public string Author { get; set; }

        [JsonPropertyName("sent_at")]
        public DateTime SentAt { get; set; }

        // Propiedad para icono visual
        public string Icon => Type switch
        {
            "email" => "✉",
            "phone" => "📞",
            "manual_note" => "📝",
            _ => "🗈"
        };
    }
}
