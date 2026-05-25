using System.Text.Json.Serialization;

public sealed record MatriculadoDto
{
    [JsonPropertyName("Rut")]
    public string? Rut { get; init; }

    [JsonPropertyName("Nombre_carrera")]
    public string? Nombre_carrera { get; init; }

    [JsonPropertyName("Estado_Academico")]
    public string? Estado_Academico { get; init; }
}
