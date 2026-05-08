using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestaoEventos.Data;
using GestaoEventos.Models;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authorization;

namespace GestaoEventos.Controllers;

//[Authorize(Roles = "Admin")] // Protege todas as ações de gerenciamento
public class EventosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EventosController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: Eventos
    public async Task<IActionResult> Index()
    {
        // ORM com Join duplo: Traz dados da Categoria E do Local
        var eventos = await _context.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Local)
            .ToListAsync();

        return View(eventos);
    }

    // GET: Eventos/Create
    public IActionResult Create()
    {
        // Carrega as duas listas para os DropDowns na View
        ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nome");
        ViewBag.LocalId = new SelectList(_context.Locais, "Id", "Nome");
        return View();
    }

    // POST: Eventos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Evento evento, IFormFile imagem)
    {
        // REMOVER as validações que impedem o IsValid de ser true
        ModelState.Remove("imagem");      // O arquivo IFormFile não é parte da Model
        ModelState.Remove("ImagemUrl");   // Vai ser preenchido manualmente abaixo
        ModelState.Remove("Categoria");   // Objeto de navegação (vem nulo do form)
        ModelState.Remove("Local");       // Objeto de navegação (vem nulo do form)

        if (ModelState.IsValid)
        {
            if (imagem != null && imagem.Length > 0)
            {
                string pastaEventos = Path.Combine(_environment.WebRootPath, "img", "eventos");

                // Cria a pasta se não existir (evita erro de diretório)
                if (!Directory.Exists(pastaEventos))
                    Directory.CreateDirectory(pastaEventos);

                string nomeUnico = Guid.NewGuid().ToString() + "_" + imagem.FileName;
                string caminhoCompleto = Path.Combine(pastaEventos, nomeUnico);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                evento.ImagemUrl = nomeUnico;
            }

            _context.Add(evento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Se der erro, recarrega as listas para a tela não quebrar
        ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nome", evento.CategoriaId);
        ViewBag.LocalId = new SelectList(_context.Locais, "Id", "Nome", evento.LocalId);
        return View(evento);
    }
    // GET: Eventos/Edit/5

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _context.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        // Carrega as listas selecionando os valores atuais do evento
        ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nome", evento.CategoriaId);
        ViewBag.LocalId = new SelectList(_context.Locais, "Id", "Nome", evento.LocalId);

        return View(evento);
    }

    // POST: Eventos/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Evento evento, IFormFile? imagem)
    {
        if (id != evento.Id) return NotFound();

        ModelState.Remove("imagem");
        ModelState.Remove("ImagemUrl");
        ModelState.Remove("Categoria");
        ModelState.Remove("Local");

        if (ModelState.IsValid)
        {
            try
            {
                if (imagem != null && imagem.Length > 0)
                {
                    string pastaEventos = Path.Combine(_environment.WebRootPath, "img", "eventos");

                    // 1. Deletar a imagem antiga se ela existir
                    if (!string.IsNullOrEmpty(evento.ImagemUrl))
                    {
                        string caminhoAntigo = Path.Combine(pastaEventos, evento.ImagemUrl);
                        if (System.IO.File.Exists(caminhoAntigo))
                            System.IO.File.Delete(caminhoAntigo);
                    }

                    // 2. Salvar a nova imagem
                    string nomeUnico = Guid.NewGuid().ToString() + "_" + imagem.FileName;
                    string caminhoCompleto = Path.Combine(pastaEventos, nomeUnico);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await imagem.CopyToAsync(stream);
                    }

                    evento.ImagemUrl = nomeUnico;
                }
                else
                {
                    // Importante: Se não enviou imagem nova, mantém a que já estava no banco
                    // Para isso, precisamos garantir que o ImagemUrl não venha nulo do Form
                    _context.Entry(evento).Property(x => x.ImagemUrl).IsModified = (evento.ImagemUrl != null);
                }

                _context.Update(evento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventoExists(evento.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nome", evento.CategoriaId);
        ViewBag.LocalId = new SelectList(_context.Locais, "Id", "Nome", evento.LocalId);
        return View(evento);
    }

    // Método auxiliar necessário para o Catch acima
    private bool EventoExists(int id) => _context.Eventos.Any(e => e.Id == id);

    // GET: Eventos/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        // Include duplo para mostrar onde e que tipo de evento está sendo excluído
        var evento = await _context.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Local)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (evento == null) return NotFound();

        return View(evento);
    }

    [Authorize(Roles = "Admin")]
    // POST: Eventos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var evento = await _context.Eventos.FindAsync(id);
        if (evento != null)
        {
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}