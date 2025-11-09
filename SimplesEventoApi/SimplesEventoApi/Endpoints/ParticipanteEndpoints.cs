using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using SimplesEventoApi.Data;
using SimplesEventoApi.Models;
namespace SimplesEventoApi.Endpoints;

public static class ParticipanteEndpoints
{
    public static void MapParticipanteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Participante").WithTags(nameof(Participante));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Participante.ToListAsync();
        })
        .WithName("GetAllParticipantes")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Participante>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Participante.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Participante model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetParticipanteById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Participante participante, AppDbContext db) =>
        {
            var affected = await db.Participante
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, participante.Id)
                    .SetProperty(m => m.NomeCompleto, participante.NomeCompleto)
                    .SetProperty(m => m.Email, participante.Email)
                    .SetProperty(m => m.IngressoId, participante.IngressoId)
                    .SetProperty(m => m.DataRegistro, participante.DataRegistro)
                    .SetProperty(m => m.StatusPagamento, participante.StatusPagamento)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateParticipante")
        .WithOpenApi();

        group.MapPost("/", async (Participante participante, AppDbContext db) =>
        {
            db.Participante.Add(participante);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Participante/{participante.Id}", participante);
        })
        .WithName("CreateParticipante")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Participante
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteParticipante")
        .WithOpenApi();

        group.MapGet("/relatorios/contagem-status", async (AppDbContext db) =>
        {
            var contagemPorStatus = await db.Participante
                .GroupBy(p => p.StatusPagamento)
                .Select(g => new
                {
                    Status = g.Key,
                    Contagem = g.Count()
                })
                .OrderByDescending(x => x.Contagem)
                .ToListAsync();

            return TypedResults.Ok(contagemPorStatus);
        })
        .WithName("GetContagemPorStatusPagamento")
        .WithOpenApi()
        .WithSummary("Retorna a contagem total de participantes agrupados por Status de Pagamento.");

        group.MapGet("/relatorios/registros-recentes", async (AppDbContext db) =>
        {
            var limiteDias = 7;
            var dataLimite = DateTimeOffset.UtcNow.AddDays(-limiteDias);

            var todosParticipantes = await db.Participante.ToListAsync();

            var registrosRecentes = todosParticipantes
                .Where(p => p.DataRegistro >= dataLimite)
                .OrderByDescending(p => p.DataRegistro)
                .Select(p => new { p.NomeCompleto, p.Email, p.DataRegistro })
                .ToList();

            return TypedResults.Ok(registrosRecentes);
        })
        .WithName("GetRegistrosRecentes")
        .WithOpenApi()
        .WithSummary("Retorna participantes registrados nos últimos 7 dias, ordenados do mais recente para o mais antigo.");
    }
}
