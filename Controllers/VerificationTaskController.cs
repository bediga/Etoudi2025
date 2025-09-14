using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class VerificationTaskController : Controller
    {
        private readonly Vc2025DbContext _context;

        public VerificationTaskController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: VerificationTask
        public async Task<IActionResult> Index()
        {
            var verificationTasks = await _context.VerificationTasks
                .Include(v => v.Checker)
                .Include(v => v.VerificationHistories)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
            return View(verificationTasks);
        }

        // GET: VerificationTask/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verificationTask = await _context.VerificationTasks
                .Include(v => v.Checker)
                .Include(v => v.VerificationHistories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (verificationTask == null)
            {
                return NotFound();
            }

            return View(verificationTask);
        }

        // GET: VerificationTask/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = await GetActiveUsers();
            ViewBag.TaskTypes = GetTaskTypes();
            ViewBag.Priorities = GetPriorities();
            return View();
        }

        // POST: VerificationTask/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SubmissionId,CheckerId,AssignedDate,Status,Priority,VerificationNotes,VerificationDecision,RejectionReason")] VerificationTask verificationTask)
        {
            if (ModelState.IsValid)
            {
                verificationTask.Status = "En attente";
                verificationTask.CreatedAt = DateTime.UtcNow;
                verificationTask.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(verificationTask);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Tâche de vérification #{verificationTask.Id} créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Users = await GetActiveUsers();
            ViewBag.TaskTypes = GetTaskTypes();
            ViewBag.Priorities = GetPriorities();
            return View(verificationTask);
        }

        // GET: VerificationTask/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verificationTask = await _context.VerificationTasks.FindAsync(id);
            if (verificationTask == null)
            {
                return NotFound();
            }
            
            ViewBag.Users = await GetActiveUsers();
            ViewBag.TaskTypes = GetTaskTypes();
            ViewBag.Priorities = GetPriorities();
            ViewBag.Statuses = GetStatuses();
            return View(verificationTask);
        }

        // POST: VerificationTask/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubmissionId,CheckerId,AssignedDate,Status,Priority,VerificationNotes,CompletionDate,VerificationDecision,RejectionReason,CreatedAt")] VerificationTask verificationTask)
        {
            if (id != verificationTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTask = await _context.VerificationTasks.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    
                    verificationTask.UpdatedAt = DateTime.UtcNow;
                    
                    // Marquer comme terminé si le statut change vers "completed"
                    if (originalTask?.Status != "completed" && verificationTask.Status == "completed")
                    {
                        verificationTask.CompletionDate = DateTime.UtcNow;
                    }
                    
                    _context.Update(verificationTask);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Tâche de vérification #{verificationTask.Id} mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VerificationTaskExists(verificationTask.Id))
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
            
            ViewBag.Users = await GetActiveUsers();
            ViewBag.TaskTypes = GetTaskTypes();
            ViewBag.Priorities = GetPriorities();
            ViewBag.Statuses = GetStatuses();
            return View(verificationTask);
        }

        // GET: VerificationTask/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verificationTask = await _context.VerificationTasks
                .Include(v => v.Checker)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (verificationTask == null)
            {
                return NotFound();
            }

            return View(verificationTask);
        }

        // POST: VerificationTask/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var verificationTask = await _context.VerificationTasks.FindAsync(id);
            
            if (verificationTask != null)
            {
                // Vérifier s'il y a des historiques associés
                var historyCount = await _context.VerificationHistories.CountAsync(h => h.TaskId == id);
                
                if (historyCount > 0)
                {
                    TempData["Error"] = $"Impossible de supprimer la tâche #{verificationTask.Id}. Elle contient un historique de vérification.";
                    return RedirectToAction(nameof(Index));
                }

                _context.VerificationTasks.Remove(verificationTask);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Tâche de vérification #{verificationTask.Id} supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: VerificationTask/Assign/5
        [HttpPost]
        public async Task<IActionResult> Assign(int id, int userId)
        {
            var verificationTask = await _context.VerificationTasks.FindAsync(id);
            
            if (verificationTask != null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilisateur introuvable.";
                    return RedirectToAction(nameof(Index));
                }

                verificationTask.CheckerId = userId;
                verificationTask.Status = "En cours";
                verificationTask.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(verificationTask);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Tâche #{verificationTask.Id} assignée à {user.FirstName} {user.LastName}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: VerificationTask/Complete/5
        [HttpPost]
        public async Task<IActionResult> Complete(int id, string notes = null)
        {
            var verificationTask = await _context.VerificationTasks.FindAsync(id);
            
            if (verificationTask != null)
            {
                verificationTask.Status = "completed";
                verificationTask.CompletionDate = DateTime.UtcNow;
                verificationTask.UpdatedAt = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    verificationTask.VerificationNotes = notes;
                }
                
                _context.Update(verificationTask);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Tâche #{verificationTask.Id} marquée comme terminée.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET VerificationTask/GetByUser?userId=xxx
        [HttpGet]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var tasks = await _context.VerificationTasks
                    .Where(v => v.CheckerId == userId)
                    .Select(v => new
                    {
                        id = v.Id,
                        submissionId = v.SubmissionId,
                        checkerId = v.CheckerId,
                        priority = v.Priority,
                        status = v.Status,
                        assignedDate = v.AssignedDate,
                        completionDate = v.CompletionDate,
                        createdAt = v.CreatedAt,
                        verificationNotes = v.VerificationNotes
                    })
                    .OrderByDescending(v => v.createdAt)
                    .ToListAsync();

                return Json(tasks);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET VerificationTask/GetByStatus?status=xxx
        [HttpGet]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                var tasks = await _context.VerificationTasks
                    .Where(v => v.Status == status)
                    .Include(v => v.Checker)
                    .Select(v => new
                    {
                        id = v.Id,
                        submissionId = v.SubmissionId,
                        priority = v.Priority,
                        assignedTo = v.Checker != null ? v.Checker.FirstName + " " + v.Checker.LastName : null,
                        assignedDate = v.AssignedDate,
                        completionDate = v.CompletionDate,
                        createdAt = v.CreatedAt,
                        verificationNotes = v.VerificationNotes
                    })
                    .OrderByDescending(v => v.createdAt)
                    .ToListAsync();

                return Json(tasks);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET VerificationTask/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.VerificationTasks
                    .GroupBy(v => 1)
                    .Select(g => new
                    {
                        totalTasks = g.Count(),
                        pendingTasks = g.Count(v => v.Status == "pending"),
                        inProgressTasks = g.Count(v => v.Status == "in_progress"),
                        completedTasks = g.Count(v => v.Status == "completed"),
                        assignedTasks = g.Count(v => v.CheckerId != null),
                        overdueTasks = 0, // Pas de logique de date limite définie
                        highPriorityTasks = g.Count(v => v.Priority == "high"),
                        mediumPriorityTasks = g.Count(v => v.Priority == "normal"),
                        lowPriorityTasks = g.Count(v => v.Priority == "low")
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalTasks = 0,
                    pendingTasks = 0,
                    inProgressTasks = 0,
                    completedTasks = 0,
                    assignedTasks = 0,
                    overdueTasks = 0,
                    highPriorityTasks = 0,
                    mediumPriorityTasks = 0,
                    lowPriorityTasks = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool VerificationTaskExists(int id)
        {
            return _context.VerificationTasks.Any(e => e.Id == id);
        }

        private async Task<List<object>> GetActiveUsers()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    role = u.Role
                })
                .OrderBy(u => u.name)
                .Cast<object>()
                .ToListAsync();
        }

        private List<string> GetTaskTypes()
        {
            return new List<string>
            {
                "Vérification de résultats",
                "Audit de procédure",
                "Contrôle de conformité",
                "Investigation d'anomalie",
                "Validation de données",
                "Inspection sur site",
                "Révision de documents"
            };
        }

        private List<string> GetPriorities()
        {
            return new List<string> { "Basse", "Moyenne", "Haute", "Critique" };
        }

        private List<string> GetStatuses()
        {
            return new List<string> { "En attente", "En cours", "Terminé", "Suspendu", "Annulé" };
        }
    }
}