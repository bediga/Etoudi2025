using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class UserController : Controller
    {
        private readonly Vc2025DbContext _context;

        public UserController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserAssociations)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserAssociations)
                .Include(u => u.CheckerTasks)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = GetRolesList();
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,Username,Role,PhoneNumber,Region,Department,PollingStationId,NationalId")] User user)
        {
            if (ModelState.IsValid)
            {
                // Vérifier l'unicité de l'email
                var existingUser = await _context.Users
                    .Where(u => u.Email == user.Email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Cette adresse email existe déjà.");
                    
                    ViewBag.Roles = GetRolesList();
                    return View(user);
                }

                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Utilisateur '{user.FirstName} {user.LastName}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Roles = GetRolesList();
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            ViewBag.Roles = GetRolesList();
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Username,Role,PhoneNumber,Region,Department,PollingStationId,NationalId,IsActive,CreatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérifier l'unicité du nom d'utilisateur et de l'email (exclure l'utilisateur actuel)
                    var existingUser = await _context.Users
                        .Where(u => u.Id != id && u.Email == user.Email)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Cette adresse email existe déjà.");
                        
                        ViewBag.Roles = GetRolesList();
                        return View(user);
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Utilisateur '{user.FirstName} {user.LastName}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Roles = GetRolesList();
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user != null)
            {
                // Vérifier s'il y a des tâches de vérification associées
                var verificationTasksCount = await _context.VerificationTasks.CountAsync(v => v.CheckerId == id);
                
                if (verificationTasksCount > 0)
                {
                    TempData["Error"] = $"Impossible de supprimer l'utilisateur '{user.FirstName} {user.LastName}'. Il a des tâches de vérification en cours.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Utilisateur '{user.FirstName} {user.LastName}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: User/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(user);
                await _context.SaveChangesAsync();
                
                var status = user.IsActive ? "activé" : "désactivé";
                TempData["Success"] = $"Utilisateur '{user.FirstName} {user.LastName}' {status} avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET User/GetByRole?role=xxx
        [HttpGet]
        public async Task<IActionResult> GetByRole(string role)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role == role && u.IsActive)
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        region = u.Region,
                        department = u.Department,
                        pollingStationId = u.PollingStationId
                    })
                    .OrderBy(u => u.firstName)
                    .ThenBy(u => u.lastName)
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET User/GetActive
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var activeUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        role = u.Role,
                        region = u.Region,
                        department = u.Department,
                        phoneNumber = u.PhoneNumber
                    })
                    .OrderBy(u => u.role)
                    .ThenBy(u => u.firstName)
                    .ToListAsync();

                return Json(activeUsers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET User/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.Users
                    .GroupBy(u => 1)
                    .Select(g => new
                    {
                        totalUsers = g.Count(),
                        activeUsers = g.Count(u => u.IsActive),
                        inactiveUsers = g.Count(u => !u.IsActive),
                        adminUsers = g.Count(u => u.Role == "Admin"),
                        supervisorUsers = g.Count(u => u.Role == "Superviseur"),
                        agentUsers = g.Count(u => u.Role == "Agent"),
                        observerUsers = g.Count(u => u.Role == "Observateur")
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalUsers = 0,
                    activeUsers = 0,
                    inactiveUsers = 0,
                    adminUsers = 0,
                    supervisorUsers = 0,
                    agentUsers = 0,
                    observerUsers = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private List<string> GetRolesList()
        {
            return new List<string>
            {
                "Admin",
                "Superviseur",
                "Agent",
                "Observateur",
                "Scrutateur",
                "Président de bureau",
                "Secrétaire"
            };
        }
    }
}