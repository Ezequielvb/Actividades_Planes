namespace ActividadesCiudad.Models;

public class Actividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double? Precio { get; set; }
    public int? Duracion { get; set; }
    public string? Tipo { get; set; }
    public string? Estado { get; set; }
}

