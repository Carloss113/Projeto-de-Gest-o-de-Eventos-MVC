using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestaoEventos.Data;
using GestaoEventos.Helpers;
using GestaoEventos.Models;

namespace GestaoEventos.Controllers;

public class IngressosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IngressosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ═══════════════════════════════════════════════════════════════
    // CRUD ADMIN — só Admin acessa
    // ═══════════════════════════════════════════════════════════════

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var ingressos = await _context.Ingressos
            .Include(i => i.Evento)
            .OrderBy(i => i.Evento!.Titulo)
            .ToListAsync();
        return View(ingressos);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.EventoId = new SelectList(_context.Eventos, "Id", "Titulo");
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ingresso ingresso)
    {
        ModelState.Remove("Evento");
        if (ModelState.IsValid)
        {
            _context.Add(ingresso);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.EventoId = new SelectList(_context.Eventos, "Id", "Titulo", ingresso.EventoId);
        return View(ingresso);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var ingresso = await _context.Ingressos.FindAsync(id);
        if (ingresso == null) return NotFound();
        ViewBag.EventoId = new SelectList(_context.Eventos, "Id", "Titulo", ingresso.EventoId);
        return View(ingresso);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Ingresso ingresso)
    {
        if (id != ingresso.Id) return NotFound();
        ModelState.Remove("Evento");
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ingresso);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ingressos.Any(i => i.Id == ingresso.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.EventoId = new SelectList(_context.Eventos, "Id", "Titulo", ingresso.EventoId);
        return View(ingresso);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var ingresso = await _context.Ingressos
            .Include(i => i.Evento)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (ingresso == null) return NotFound();
        return View(ingresso);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ingresso = await _context.Ingressos.FindAsync(id);
        if (ingresso != null) { _context.Ingressos.Remove(ingresso); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUXO DE COMPRA — cliente logado
    // ═══════════════════════════════════════════════════════════════

    // GET: /Ingressos/Detalhes/5 — público
    public async Task<IActionResult> Detalhes(int id)
    {
        var evento = await _context.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Local)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null) return NotFound();

        var ingressos = await _context.Ingressos
            .Where(i => i.EventoId == id && i.Ativo)
            .ToListAsync();

        ViewBag.Ingressos = ingressos;
        ViewBag.TotalCarrinho = SessionHelper.TotalItens(HttpContext.Session);

        return View(evento);
    }

    // POST: /Ingressos/AdicionarAoCarrinho
    [Authorize, HttpPost]
    public async Task<IActionResult> AdicionarAoCarrinho(int ingressoId, int quantidade)
    {
        var ingresso = await _context.Ingressos
            .Include(i => i.Evento)
            .FirstOrDefaultAsync(i => i.Id == ingressoId);

        if (ingresso == null) return NotFound();

        if (quantidade < 1 || quantidade > ingresso.QuantidadeRestante)
        {
            TempData["Erro"] = $"Quantidade inválida. Disponível: {ingresso.QuantidadeRestante}";
            return RedirectToAction("Detalhes", new { id = ingresso.EventoId });
        }

        var item = new CarrinhoItem
        {
            EventoId = ingresso.EventoId,
            IngressoId = ingresso.Id,
            TituloEvento = ingresso.Evento!.Titulo,     
            TipoIngresso = ingresso.Tipo.ToString(),
            ImagemUrl = ingresso.Evento.ImagemUrl, 
            DataEvento = ingresso.Evento.Data,
            Preco = ingresso.Preco,
            Quantidade = quantidade
        };

        SessionHelper.AdicionarItem(HttpContext.Session, item);

        TempData["Sucesso"] = $"{quantidade} ingresso(s) {ingresso.Tipo} de \"{ingresso.Evento.Titulo}\" adicionado(s)!";
        return RedirectToAction("Carrinho");
    }

    // GET: /Ingressos/Carrinho
    [Authorize]
    public IActionResult Carrinho()
    {
        var carrinho = SessionHelper.ObterCarrinho(HttpContext.Session);
        ViewBag.TotalCarrinho = carrinho.Sum(i => i.Quantidade);
        return View(carrinho);
    }

    // POST: /Ingressos/RemoverDoCarrinho
    [Authorize, HttpPost]
    public IActionResult RemoverDoCarrinho(int ingressoId)
    {
        SessionHelper.RemoverItem(HttpContext.Session, ingressoId);
        return RedirectToAction("Carrinho");
    }

    // GET: /Ingressos/Checkout
    [Authorize]
    public IActionResult Checkout()
    {
        var carrinho = SessionHelper.ObterCarrinho(HttpContext.Session);
        if (!carrinho.Any()) return RedirectToAction("Carrinho");
        ViewBag.Total = carrinho.Sum(i => i.Subtotal);
        ViewBag.TotalCarrinho = carrinho.Sum(i => i.Quantidade);
        return View(carrinho);
    }

    // POST: /Ingressos/FinalizarCompra
    [Authorize, HttpPost]
    public async Task<IActionResult> FinalizarCompra()
    {
        var carrinho = SessionHelper.ObterCarrinho(HttpContext.Session);
        if (!carrinho.Any()) return RedirectToAction("Carrinho");

        var userId = _userManager.GetUserId(User)!;
        var userEmail = _userManager.GetUserName(User)!;

        foreach (var item in carrinho)
        {
            var ingresso = await _context.Ingressos.FindAsync(item.IngressoId);
            if (ingresso == null || ingresso.QuantidadeRestante < item.Quantidade)
            {
                TempData["Erro"] = $"Ingresso \"{item.TipoIngresso}\" de \"{item.TituloEvento}\" sem estoque suficiente.";
                return RedirectToAction("Carrinho");
            }
            ingresso.QuantidadeVendida += item.Quantidade;
        }

        var pedido = new Pedido
        {
            UserId = userId,
            UserEmail = userEmail,
            DataPedido = DateTime.Now,
            Total = carrinho.Sum(i => i.Subtotal),
            Status = StatusPedido.Confirmado,
            Itens = carrinho.Select(i => new PedidoItem
            {
                IngressoId = i.IngressoId,
                NomeEvento = i.TituloEvento,
                TipoIngresso = i.TipoIngresso,
                PrecoUnitario = i.Preco,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        SessionHelper.LimparCarrinho(HttpContext.Session);

        return RedirectToAction("Confirmacao", new { pedidoId = pedido.Id });
    }

    // GET: /Ingressos/Confirmacao/5
    [Authorize]
    public async Task<IActionResult> Confirmacao(int pedidoId)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
                .ThenInclude(i => i.Ingresso)
            .FirstOrDefaultAsync(p => p.Id == pedidoId && p.UserId == _userManager.GetUserId(User));

        if (pedido == null) return NotFound();

        ViewBag.TotalCarrinho = 0;
        return View(pedido);
    }
}