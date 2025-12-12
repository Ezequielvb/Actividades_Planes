namespace ActividadesCiudad.Models;

public class Ciudad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? Poblacion { get; set; }
    public string? Pais { get; set; }
    public string? FechaFundacion { get; set; }
}

