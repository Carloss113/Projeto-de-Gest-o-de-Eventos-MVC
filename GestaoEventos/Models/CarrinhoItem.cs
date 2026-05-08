using System.ComponentModel.DataAnnotations;

namespace GestaoEventos.Models
{
    public class CarrinhoItem
    {
        public int EventoId { get; set; }
        public int IngressoId { get; set; }
        public string TituloEvento { get; set; } = string.Empty;
        public string TipoIngresso { get; set; } = string.Empty; //Inteira; Meia; Cortesia
        public string? ImagemUrl { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public decimal Subtotal => Preco * Quantidade;
    }
}