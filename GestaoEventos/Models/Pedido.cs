using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoEventos.Models;

public enum StatusPedido
{
    Pendente,
    Confirmado,
    Cancelado
}

public class Pedido
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Display(Name = "Email do comprador")]
    public string UserEmail { get; set; } = string.Empty;

    [Display(Name = "Data do Pedido")]
    public DateTime DataPedido { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    public StatusPedido Status { get; set; } = StatusPedido.Confirmado;

    public List<PedidoItem> Itens { get; set; } = new();
}

public class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int IngressoId { get; set; }
    public Ingresso? Ingresso { get; set; }

    [Required]
    public string NomeEvento { get; set; } = string.Empty;

    public string TipoIngresso { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoUnitario { get; set; }

    public int Quantidade { get; set; }

    public decimal Subtotal => PrecoUnitario * Quantidade;
}
