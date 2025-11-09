using System.ComponentModel.DataAnnotations;

namespace SimplesEventoApi.Models;

public class Local
{
    [Key]
    public int Id { get; set; } 

    public string Nome { get; set; }
    public string Endereco { get; set; }
    public int CapacidadeMaxima { get; set; }
}
