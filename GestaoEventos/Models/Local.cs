using System.ComponentModel.DataAnnotations;

namespace GestaoEventos.Models
{

    public class Local
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório!")]
        public string Nome { get; set; }

        public int Capacidade { get; set; }
    }
}
