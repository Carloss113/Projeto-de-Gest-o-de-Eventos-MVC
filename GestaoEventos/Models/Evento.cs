using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GestaoEventos.Models
{
    public class Evento
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "O Título é obrigatório!")]
        [StringLength(100)]
        public string Titulo { get; set; }

        [Display(Name ="Data do Evento")]
        public DateTime Data { get; set; }

        // Relacionamento com Categorias
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Relacionamento com Local
        [Required(ErrorMessage = "O Local é obrigatório!")]
        public int LocalId { get; set; }
        public Local? Local { get; set; }
        public string? ImagemUrl { get; set; }
        public List<Ingresso> Ingressos { get; set; }
    }
}
