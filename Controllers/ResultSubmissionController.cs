using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class ResultSubmissionController : Controller
    {
        private readonly Vc2025DbContext _context;

        public ResultSubmissionController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: ResultSubmission
        public async Task<IActionResult> Index()
        {
            var resultSubmissions = await _context.ResultSubmissions
                .Include(r => r.ResultSubmissionDetails)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
            return View(resultSubmissions);
        }

        // GET: ResultSubmission/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions
                .Include(r => r.ResultSubmissionDetails)
                .Include(r => r.SubmissionDocuments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resultSubmission == null)
            {
                return NotFound();
            }

            return View(resultSubmission);
        }

        // GET: ResultSubmission/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ResultSubmission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,SubmittedBy,VotersCount,VotesCount,BlankVotes,InvalidVotes,ValidVotes,Observations")] ResultSubmission resultSubmission)
        {
            if (ModelState.IsValid)
            {
                // Validation simple : les votes totaux ne peuvent pas dépasser les électeurs inscrits  
                if (resultSubmission.TotalVotes > resultSubmission.RegisteredVoters)
                {
                    ModelState.AddModelError("VotesCount", 
                        "Le nombre de votes ne peut pas dépasser le nombre d'électeurs inscrits.");
                    return View(resultSubmission);
                }

                resultSubmission.Status = "submitted";
                resultSubmission.SubmittedAt = DateTime.UtcNow;
                
                _context.Add(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Soumission de résultats créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(resultSubmission);
        }

        // GET: ResultSubmission/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            if (resultSubmission == null)
            {
                return NotFound();
            }

            // Ne permettre la modification que si pas encore validé
            if (resultSubmission.Status == "Validé")
            {
                TempData["Error"] = "Impossible de modifier une soumission déjà validée.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            return View(resultSubmission);
        }

        // POST: ResultSubmission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,SubmittedBy,VotersCount,VotesCount,BlankVotes,InvalidVotes,ValidVotes,Observations,Status,SubmittedAt,CreatedAt")] ResultSubmission resultSubmission)
        {
            if (id != resultSubmission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérification simple
                    if (resultSubmission.TotalVotes > resultSubmission.RegisteredVoters)
                    {
                        ModelState.AddModelError("TotalVotes", 
                            "Le nombre de votes ne peut pas dépasser le nombre d'électeurs inscrits.");
                        return View(resultSubmission);
                    }
                    
                    _context.Update(resultSubmission);
                    await _context.SaveChangesAsync();
                    
                                        TempData["Success"] = "Soumission de résultats mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResultSubmissionExists(resultSubmission.Id))
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
            
            return View(resultSubmission);
        }

        // GET: ResultSubmission/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resultSubmission == null)
            {
                return NotFound();
            }

            return View(resultSubmission);
        }

        // POST: ResultSubmission/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            
            if (resultSubmission != null)
            {
                // Vérifier si la soumission est validée
                if (resultSubmission.Status == "Validé")
                {
                    TempData["Error"] = $"Impossible de supprimer la soumission pour '{resultSubmission.PollingStation?.Name ?? "N/A"}'. Elle est déjà validée.";
                    return RedirectToAction(nameof(Index));
                }

                _context.ResultSubmissions.Remove(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Soumission pour '{resultSubmission.PollingStation?.Name ?? "N/A"}' supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ResultSubmission/ChangeStatus/5
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus, string reviewedBy = null)
        {
            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            
            if (resultSubmission != null)
            {
                var oldStatus = resultSubmission.Status;
                resultSubmission.Status = newStatus;

                if (newStatus == "verified")
                {
                    if (int.TryParse(reviewedBy, out int userId))
                    {
                        resultSubmission.VerifiedBy = userId;
                    }
                    resultSubmission.VerifiedAt = DateTime.UtcNow;
                }
                else if (newStatus == "Rejeté")
                {
                    if (int.TryParse(reviewedBy, out int userId))
                    {
                        resultSubmission.VerifiedBy = userId;
                    }
                    resultSubmission.VerifiedAt = DateTime.UtcNow;
                }
                
                _context.Update(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Statut de la soumission '{resultSubmission.PollingStation?.Name ?? "N/A"}' changé de '{oldStatus}' à '{newStatus}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET ResultSubmission/GetByStatus?status=xxx
        [HttpGet]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                // Simplification temporaire pour éviter les erreurs de navigation complexes
                var rawSubmissions = await _context.ResultSubmissions
                    .Include(r => r.PollingStation)
                    .Where(r => r.Status == status)
                    .ToListAsync();

                var submissions = rawSubmissions.Select(r => new
                {
                    id = r.Id,
                    pollingStationName = r.PollingStation?.Name ?? "N/A",
                    region = "N/A", // Simplification temporaire
                    department = "N/A", // Simplification temporaire
                    commune = "N/A", // Simplification temporaire
                    submittedBy = r.SubmittedBy,
                    submittedAt = r.SubmittedAt,
                    votersCount = r.RegisteredVoters,
                    votesCount = r.TotalVotes,
                    validVotes = r.TotalVotes,
                    reviewedBy = r.VerifiedBy,
                    reviewedAt = r.VerifiedAt
                })
                .OrderByDescending(r => r.submittedAt)
                .ToList();

                return Json(submissions);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET ResultSubmission/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.ResultSubmissions
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        totalSubmissions = g.Count(),
                        submittedCount = g.Count(r => r.Status == "Soumis"),
                        validatedCount = g.Count(r => r.Status == "Validé"),
                        rejectedCount = g.Count(r => r.Status == "Rejeté"),
                        totalVoters = g.Sum(r => r.RegisteredVoters),
                        totalVotes = g.Sum(r => r.TotalVotes),
                        totalValidVotes = g.Sum(r => r.TotalVotes),
                        overallTurnout = g.Sum(r => r.RegisteredVoters) > 0 ? 
                            (double)g.Sum(r => r.TotalVotes) / g.Sum(r => r.RegisteredVoters) * 100 : 0
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalSubmissions = 0,
                    submittedCount = 0,
                    validatedCount = 0,
                    rejectedCount = 0,
                    totalVoters = 0,
                    totalVotes = 0,
                    totalValidVotes = 0,
                    overallTurnout = 0.0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool ResultSubmissionExists(int id)
        {
            return _context.ResultSubmissions.Any(e => e.Id == id);
        }
    }
}
