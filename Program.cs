using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Middleware de API Key ──────────────────────────────────────────
// Protege todos los endpoints /api/* con el header X-Api-Key
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var expectedKey = app.Configuration["ApiSettings:ApiKey"];
        context.Request.Headers.TryGetValue("X-Api-Key", out var receivedKey);

        if (string.IsNullOrWhiteSpace(receivedKey) || receivedKey != expectedKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "No autorizado. Debes enviar un header X-Api-Key válido."
            });
            return;
        }
    }
    await next();
});
// ─────────────────────────────────────────────────────────────────

app.MapGet("/api/vista", async (IConfiguration config, string? estado) =>
{
    var connectionString = config.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("No está configurada ConnectionStrings:SqlServer", statusCode: 500);
    }

    var viewName = config["ApiSettings:ViewName"] ?? "dbo.vw_TuVista";

    if (!Regex.IsMatch(viewName, "^[A-Za-z0-9_\\.\\[\\]]+$"))
    {
        return Results.BadRequest("El nombre de la vista configurado no es válido.");
    }

    var top = 100;
    if (int.TryParse(config["ApiSettings:Top"], out var parsedTop) && parsedTop > 0 && parsedTop <= 1000)
    {
        top = parsedTop;
    }

    // Filtro opcional por Estado Academico (campo con espacio → corchetes en SQL)
    var sql = $"SELECT TOP ({top}) Rut, Nombre_carrera, [Estado Academico] FROM {viewName}";
    if (!string.IsNullOrWhiteSpace(estado))
    {
        sql += " WHERE [Estado Academico] = @estado";
    }

    var rows = new List<MatriculadoDto>();

    try
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            cmd.Parameters.AddWithValue("@estado", estado);
        }

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rows.Add(new MatriculadoDto
            {
                Rut            = await reader.IsDBNullAsync(0) ? null : reader.GetString(0),
                Nombre_carrera = await reader.IsDBNullAsync(1) ? null : reader.GetString(1),
                Estado_Academico = await reader.IsDBNullAsync(2) ? null : reader.GetString(2)
            });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error consultando SQL Server: {ex.Message}", statusCode: 500);
    }

    return Results.Ok(rows);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
// Estes es un comentario de prueba
// Mi segunda linea
// Tercera linea
app.Run();
