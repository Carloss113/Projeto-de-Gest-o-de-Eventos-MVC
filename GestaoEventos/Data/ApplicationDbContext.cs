using GestaoEventos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GestaoEventos.Data;
public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Passo 2: DECLARAR AS MODELS
    public DbSet<Categoria> Categorias { get;set; }
    public DbSet<Local> Locais { get;set; }
    public DbSet<Evento> Eventos { get;set; }
    // primeiro vem o nome da Model e depois como nós
    // iremos chamar na nossa solução e no BD
    public DbSet<Ingresso> Ingressos { get;set; }
    public DbSet<Pedido> Pedidos { get;set; }
    public DbSet<PedidoItem> PedidoItems { get;set; }
}
