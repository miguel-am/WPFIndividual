using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopApp.Models
{
    public class CambioEstado
    {
        public string Propiedad { get; set; } = "";
        public string Valor { get; set; } = "";
    }

    public class AuditLog
    {
        [JsonPropertyName("booking_id")]
        public JsonElement? BookingData { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("actor_id")]
        public JsonElement? ActorData { get; set; }

        [JsonPropertyName("actor_type")]
        public string? ActorType { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("previous_state")]
        public JsonElement? PreviousState { get; set; }

        [JsonPropertyName("new_state")]
        public JsonElement? NewState { get; set; }

        public string NombreActor
        {
            get
            {
                if (ActorData?.ValueKind != JsonValueKind.Object) return "Sistema";
                var el = ActorData.Value;
                string n = el.TryGetProperty("firstName", out var nom) ? nom.GetString() ?? "" : "";
                string a = el.TryGetProperty("lastName", out var ape) ? ape.GetString() ?? "" : "";
                return $"{n} {a}".Trim();
            }
        }

        public string ClienteReserva
        {
            get
            {
                try
                {
                    if (BookingData?.ValueKind == JsonValueKind.Object &&
                  BookingData.Value.TryGetProperty("userId", out var user))
                    {
                        if (user.ValueKind == JsonValueKind.Object)
                        {
                            string n = user.TryGetProperty("firstName", out var nom) ? nom.GetString() ?? "" : "";
                            string a = user.TryGetProperty("lastName", out var ape) ? ape.GetString() ?? "" : "";
                            return $"{n} {a}".Trim();
                        }
                        return user.ToString() ?? "N/A";
                    }
                    return "Sin reserva";
                }
                catch { return "N/A"; }
            }
        }

        public string ActorRole
        {
            get
            {
                try
                {
                    if (BookingData?.ValueKind == JsonValueKind.Object &&
                        BookingData.Value.TryGetProperty("userId", out var user))
                    {
                        if (user.ValueKind == JsonValueKind.Object &&
                            user.TryGetProperty("role", out var roleElement))
                        {
                            string role = roleElement.GetString()?.ToLower() ?? "";

                            // Mapeo estético
                            return role switch
                            {
                                "Admin" => "Administrador",
                                "Trabajador" => "Empleado",
                                "Usuario" => "Cliente",
                                _ => role // Si no coincide, devuelve el valor original
                            };
                        }
                    }

                    return "Sistema";
                }
                catch
                {
                    return "N/A";
                }
            }
        }

        public string HabitacionesReserva
        {
            get
            {
                try
                {
                    if (BookingData?.ValueKind == JsonValueKind.Object &&
                        BookingData.Value.TryGetProperty("roomIds", out var rooms))
                    {
                        if (rooms.ValueKind == JsonValueKind.Array)
                        {
                            var listaNumeros = new List<string>();
                            foreach (var room in rooms.EnumerateArray())
                            {
                                if (room.TryGetProperty("roomNumber", out var num))
                                {
                                    listaNumeros.Add(num.ToString());
                                }
                            }
                            return listaNumeros.Count > 0 ? string.Join(", ", listaNumeros) : "Sin nms";
                        }
                    }
                    return "N/A";
                }
                catch { return "Error habs"; }
            }
        }

        public DateTime? FechaReserva => ExtraerFechaReserva();

        private DateTime? ExtraerFechaReserva()
        {
            try
            {
                // Intentamos sacar la fecha 'In' del objeto guardado
                if (NewState?.TryGetProperty("In", out var fecha) == true)
                    return fecha.GetDateTime();
                if (PreviousState?.TryGetProperty("In", out var fechaOld) == true)
                    return fechaOld.GetDateTime();
                return null;
            }
            catch { return null; }
        }
        public List<CambioEstado> OldStateList => JsonToList(PreviousState);
        public List<CambioEstado> NewStateList => JsonToList(NewState);

        private List<CambioEstado> JsonToList(JsonElement? element)
        {
            var lista = new List<CambioEstado>();
            if (element == null || element.Value.ValueKind != JsonValueKind.Object) return lista;

            foreach (var prop in element.Value.EnumerateObject())
            {
                if (prop.Name == "_id" || prop.Name == "__v") continue;
                lista.Add(new CambioEstado { Propiedad = prop.Name, Valor = prop.Value.ToString() });
            }
            return lista;
        }
    }
}