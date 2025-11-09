using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using SimplesEventoApi.Data;
using SimplesEventoApi.Models;
using SimplesEventoApi.DTOS;
namespace SimplesEventoApi.Endpoints;

public static class EventoEndpoints
{
    public static void MapEventoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Evento").WithTags(nameof(Evento));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Evento.ToListAsync();
        })
        .WithName("GetAllEventos")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Evento>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Evento.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Evento model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetEventoById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Evento evento, AppDbContext db) =>
        {
            var affected = await db.Evento
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, evento.Id)
                    .SetProperty(m => m.Titulo, evento.Titulo)
                    .SetProperty(m => m.Descricao, evento.Descricao)
                    .SetProperty(m => m.DataHoraInicio, evento.DataHoraInicio)
                    .SetProperty(m => m.DataHoraFim, evento.DataHoraFim)
                    .SetProperty(m => m.LocalId, evento.LocalId)
                    .SetProperty(m => m.Status, evento.Status)
                    .SetProperty(m => m.DataCriacao, evento.DataCriacao)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateEvento")
        .WithOpenApi();

        group.MapPost("/", async (Evento evento, AppDbContext db) =>
        {
            db.Evento.Add(evento);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Evento/{evento.Id}", evento);
        })
        .WithName("CreateEvento")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Evento
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteEvento")
        .WithOpenApi();

        group.MapGet("/relatorios/filtrar", async (
           AppDbContext db,
           [AsParameters] EventoFilterParameters filters) =>
        {
            var query = db.Evento.AsQueryable();

            if (filters.LocalId.HasValue && filters.LocalId > 0)
            {
                query = query.Where(e => e.LocalId == filters.LocalId.Value);
            }

            if (!string.IsNullOrEmpty(filters.Status))
            {
                query = query.Where(e => e.Status.ToLower() == filters.Status.ToLower());
            }

            var resultadosFiltrados = await query.ToListAsync();

            return TypedResults.Ok(resultadosFiltrados.OrderBy(e => e.DataHoraInicio));
        })
       .WithName("GetEventosFiltrados")
       .WithOpenApi()
       .WithSummary("Retorna eventos filtrados opcionalmente por LocalId e/ou Status.");

        group.MapGet("/relatorios/proximos-30-dias", async (AppDbContext db) =>
        {
            var now = DateTimeOffset.UtcNow;
            var trintaDias = now.AddDays(30);

            var todosEventos = await db.Evento.ToListAsync();

            var resultados = todosEventos
                .Where(e => e.DataHoraInicio >= now && e.DataHoraInicio <= trintaDias)
                .OrderBy(e => e.DataHoraInicio);

            return TypedResults.Ok(resultados);
        });

        group.MapGet("/relatorios/contagem-status", async (AppDbContext db) =>
        {
            var contagemPorStatus = await db.Evento
                .GroupBy(e => e.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Contagem = g.Count()
                })
                .OrderByDescending(x => x.Contagem)
                .ToListAsync();

            return TypedResults.Ok(contagemPorStatus);
        })
        .WithName("GetContagemEventosPorStatus")
        .WithOpenApi()
        .WithSummary("Retorna a contagem total de eventos agrupados por seu Status.");
    }
}
