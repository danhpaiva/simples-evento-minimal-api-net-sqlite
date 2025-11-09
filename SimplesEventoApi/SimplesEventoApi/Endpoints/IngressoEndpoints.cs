using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using SimplesEventoApi.Data;
using SimplesEventoApi.Models;
namespace SimplesEventoApi.Endpoints;

public static class IngressoEndpoints
{
    public static void MapIngressoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Ingresso").WithTags(nameof(Ingresso));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Ingresso.ToListAsync();
        })
        .WithName("GetAllIngressos")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Ingresso>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Ingresso.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Ingresso model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetIngressoById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Ingresso ingresso, AppDbContext db) =>
        {
            var affected = await db.Ingresso
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, ingresso.Id)
                    .SetProperty(m => m.Nome, ingresso.Nome)
                    .SetProperty(m => m.EventoId, ingresso.EventoId)
                    .SetProperty(m => m.Preco, ingresso.Preco)
                    .SetProperty(m => m.QuantidadeDisponivel, ingresso.QuantidadeDisponivel)
                    .SetProperty(m => m.DataVendaInicio, ingresso.DataVendaInicio)
                    .SetProperty(m => m.DataVendaFim, ingresso.DataVendaFim)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateIngresso")
        .WithOpenApi();

        group.MapPost("/", async (Ingresso ingresso, AppDbContext db) =>
        {
            db.Ingresso.Add(ingresso);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Ingresso/{ingresso.Id}", ingresso);
        })
        .WithName("CreateIngresso")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Ingresso
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteIngresso")
        .WithOpenApi();

        group.MapGet("/relatorios/receita-potencial/{eventoId}", async (int eventoId, AppDbContext db) =>
        {
            var receitaPotencial = await db.Ingresso
                .Where(i => i.EventoId == eventoId)
                .SumAsync(i => (double)(i.Preco * i.QuantidadeDisponivel));
            var resultado = new
            {
                EventoId = eventoId,
                ReceitaPotencialTotal = receitaPotencial
            };

            return TypedResults.Ok(resultado);
        })
         .WithName("GetReceitaPotencial")
         .WithOpenApi()
         .WithSummary("Calcula a receita potencial máxima (Preço * Quantidade Disponível) para um evento.");

        group.MapGet("/relatorios/vendas-abertas", async (AppDbContext db) =>
        {
            var now = DateTimeOffset.UtcNow;
            var todosIngressos = await db.Ingresso.ToListAsync();

            var ingressosAbertos = todosIngressos
                .Where(i => i.DataVendaInicio <= now && i.DataVendaFim >= now)
                .OrderBy(i => i.DataVendaFim)
                .ToList();

            return TypedResults.Ok(ingressosAbertos);
        })
             .WithName("GetIngressosVendasAbertas")
             .WithOpenApi()
             .WithSummary("Retorna todos os tipos de ingressos que estão no período ativo de vendas.");
    }
}
