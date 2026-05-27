using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class Reservations
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("userId")]
        public string User { get; set; }

        [JsonIgnore] // No se envía al backend
        public List<Rooms> Rooms { get; set; } = new List<Rooms>();

        [JsonPropertyName("roomIds")]
        public List<string> RoomIds { get; set; } = new List<string>();

        [JsonPropertyName("In")]
        public DateTime CheckIn { get; set; }

        [JsonPropertyName("Out")]
        public DateTime CheckOut { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("totalPrice")]
        public float TotalPrice { get; set; }

        [JsonPropertyName("numGuests")] 
        public int NumGuests { get; set; }

        [JsonIgnore]
        public string UserDNI { get; set; }

        [JsonIgnore]
        public string UserNombre { get; set; }

        public string RoomNumbers
        {
            get
            {
                return Rooms != null && Rooms.Any()
                    ? string.Join(", ", Rooms.Select(r => r.numRoom))
                    : "";
            }
        }

    }
}
