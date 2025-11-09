using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimplesEventoApi.Models;

public class Ingresso
{
    [Key]
    public int Id { get; set; }

    public string Nome { get; set; }

    [ForeignKey("Evento")]
    public int EventoId { get; set; }

    public decimal Preco { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public DateTimeOffset DataVendaInicio { get; set; }
    public DateTimeOffset DataVendaFim { get; set; }
}
