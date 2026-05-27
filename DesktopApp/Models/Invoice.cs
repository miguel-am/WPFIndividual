using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class Invoice
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; } = string.Empty;

        [JsonPropertyName("invoice_number")]
        public string? InvoiceNumber { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("totalPrice")]
        public double? Total { get; set; }

        [JsonPropertyName("Out")]
        public DateTime? Fecha { get; set; }

        // Este objeto recibirá los datos del populate de Node.js
        [JsonPropertyName("userId")]
        public UserData? Usuario { get; set; }

        [JsonPropertyName("numRoom")]
        public string? NumeroHabitacion { get; set; }

        // Propiedad calculada para que el filtro funcione
        public string ClienteNombre => Usuario != null 
            ? $"{Usuario.FirstName} {Usuario.LastName}" 
            
            : "Desconocido";
    }

    public class UserData
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("dni")] 
        public string? Dni { get; set; }
    }
}
