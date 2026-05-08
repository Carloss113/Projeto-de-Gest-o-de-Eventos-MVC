using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GestaoEventos.Controllers
{
    // Nível de acesso / quem está permitido acessar!
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var usersWithRoles = new List<(IdentityUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add((user, roles));
            }

            return View(usersWithRoles);
        }

        [HttpPost]
        public async Task<IActionResult> PromoverAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin") || !await _userManager.IsInRoleAsync(user, "Dono"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            TempData["Mensagem"] = $"Usuario {user.Email} foi promovido a Admin!";
            TempData["TipoMensagem"] = "success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Dono, Admin")]
        public async Task<IActionResult> RebaixarUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Impede que o Admin se rebaixe
            var usuarioLogado = await _userManager.GetUserAsync(User);
            if (usuarioLogado?.Id == userId)
            {
                TempData["Mensagem"] = "Você não pode rebaixar a si mesmo!";
                TempData["TipoMensagem"] = "danger";
                return RedirectToAction("Index");
            }

            if (await _userManager.IsInRoleAsync(user, "Dono"))
            {
                TempData["Mensagem"] = "Não é possível rebaixar um gestor/dono!";
                TempData["TipoMensagem"] = "danger";
                return RedirectToAction("Index");
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "User");

            TempData["Mensagem"] = $"Usuário {user.Email} foi rebaixado para User(Padrão)!";
            TempData["TipoMensagem"] = "warning";

            return RedirectToAction("Index");
        }
    }
}