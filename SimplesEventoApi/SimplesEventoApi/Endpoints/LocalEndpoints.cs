using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using SimplesEventoApi.Data;
using SimplesEventoApi.Models;
namespace SimplesEventoApi.Endpoints;

public static class LocalEndpoints
{
    public static void MapLocalEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Local").WithTags(nameof(Local));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Local.ToListAsync();
        })
        .WithName("GetAllLocals")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Local>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Local.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Local model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetLocalById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Local local, AppDbContext db) =>
        {
            var affected = await db.Local
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, local.Id)
                    .SetProperty(m => m.Nome, local.Nome)
                    .SetProperty(m => m.Endereco, local.Endereco)
                    .SetProperty(m => m.CapacidadeMaxima, local.CapacidadeMaxima)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateLocal")
        .WithOpenApi();

        group.MapPost("/", async (Local local, AppDbContext db) =>
        {
            db.Local.Add(local);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Local/{local.Id}",local);
        })
        .WithName("CreateLocal")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Local
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteLocal")
        .WithOpenApi();

        group.MapGet("/relatorios/capacidade-total", async (AppDbContext db) =>
        {
            var capacidadeTotal = await db.Local.SumAsync(l => l.CapacidadeMaxima);

            var resultado = new
            {
                CapacidadeTotalDeTodosLocais = capacidadeTotal,
                TotalDeLocaisCadastrados = await db.Local.CountAsync()
            };

            return TypedResults.Ok(resultado);
        })
        .WithName("GetCapacidadeTotal")
        .WithOpenApi()
        .WithSummary("Calcula a soma total da Capacidade Máxima de todos os locais.");

        group.MapGet("/relatorios/top-5-maiores", async (AppDbContext db) =>
        {
            var top5 = await db.Local
                .OrderByDescending(l => l.CapacidadeMaxima)
                .Take(5)
                .Select(l => new { l.Nome, l.CapacidadeMaxima, l.Endereco })
                .ToListAsync();

            return TypedResults.Ok(top5);
        })
        .WithName("GetTop5MaioresLocais")
        .WithOpenApi()
        .WithSummary("Retorna os 5 locais com a maior Capacidade Máxima.");
    }
}
