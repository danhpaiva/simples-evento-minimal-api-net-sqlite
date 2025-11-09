using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimplesEventoApi.Models;

public class Evento
{
    [Key]
    public int Id { get; set; }

    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }

    [ForeignKey("Local")]
    public int LocalId { get; set; }

    public string Status { get; set; } = "Aberto";
    public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.UtcNow;
}
