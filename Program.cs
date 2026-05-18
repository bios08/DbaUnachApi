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

app.MapGet("/api/vista", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("No est? configurada ConnectionStrings:SqlServer", statusCode: 500);
    }

    var viewName = config["ApiSettings:ViewName"] ?? "dbo.vw_TuVista";

    if (!Regex.IsMatch(viewName, "^[A-Za-z0-9_\\.\\[\\]]+$"))
    {
        return Results.BadRequest("El nombre de la vista configurado no es v?lido.");
    }

    var top = 100;
    if (int.TryParse(config["ApiSettings:Top"], out var parsedTop) && parsedTop > 0 && parsedTop <= 1000)
    {
        top = parsedTop;
    }

    var sql = $"SELECT TOP ({top}) * FROM {viewName};";
    var rows = new List<Dictionary<string, object?>>();

    try
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(reader.FieldCount);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
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
app.Run();
