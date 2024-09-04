namespace ContactoAPI.Models
{
    public class Contacto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Comentarios { get; set; }
        public byte[]? Adjunto { get; set; }
        public string? AdjuntoNombre { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}