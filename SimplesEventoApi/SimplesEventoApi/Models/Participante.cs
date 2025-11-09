using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimplesEventoApi.Models;

public class Participante
{
    [Key]
    public int Id { get; set; }

    public string NomeCompleto { get; set; }
    public string Email { get; set; }

    [ForeignKey("Ingresso")]
    public int IngressoId { get; set; }

    public DateTimeOffset DataRegistro { get; set; } = DateTimeOffset.UtcNow;
    public string StatusPagamento { get; set; } = "Aprovado";
}
