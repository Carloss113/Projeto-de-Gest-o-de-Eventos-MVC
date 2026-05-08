using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoEventos.Models;

public enum TipoIngresso
{
    Inteira,
    MeiaEntrada,
    Cortesia
}

public class Ingresso
{
    public int Id { get; set; }

    [Required]
    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    [Required]
    [Display(Name = "Tipo de Ingresso")]
    public TipoIngresso Tipo { get; set; }

    [Required]
    [Display(Name = "Preço")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; }

    [Required]
    [Display(Name = "Quantidade Disponível")]
    public int QuantidadeDisponivel { get; set; }

    [Display(Name = "Quantidade Vendida")]
    public int QuantidadeVendida { get; set; } = 0;

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    // Calculados — não vão para o banco
    public int QuantidadeRestante => QuantidadeDisponivel - QuantidadeVendida;
    public bool Disponivel => Ativo && QuantidadeRestante > 0;
}