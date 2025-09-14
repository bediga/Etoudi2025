using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    [Authorize]
    public class ElectionResultController : Controller
    {
        private readonly Vc2025DbContext _context;

        public ElectionResultController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: ElectionResult
        public async Task<IActionResult> Index()
        {
            var electionResults = await _context.ElectionResults
                .Include(e => e.Candidate)
                .Include(e => e.PollingStation)
                .OrderBy(e => e.Candidate.FirstName)
                .ThenBy(e => e.Candidate.LastName)
                .ToListAsync();
            return View(electionResults);
        }

        // GET: ElectionResult/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electionResult = await _context.ElectionResults
                .Include(e => e.Candidate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (electionResult == null)
            {
                return NotFound();
            }

            return View(electionResult);
        }

        // GET: ElectionResult/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Candidates = await GetActiveCandidates();
            ViewBag.PollingStations = await GetPollingStations();
            return View();
        }

        // POST: ElectionResult/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CandidateId,Votes,PollingStationId")] ElectionResult electionResult)
        {
            if (ModelState.IsValid)
            {
                // Vérifier qu'il n'y a pas déjà un résultat pour ce candidat dans ce bureau
                var existingResult = await _context.ElectionResults
                    .Where(e => e.CandidateId == electionResult.CandidateId && 
                               e.PollingStationId == electionResult.PollingStationId)
                    .FirstOrDefaultAsync();

                if (existingResult != null)
                {
                    ModelState.AddModelError("", "Un résultat existe déjà pour ce candidat dans ce bureau de vote.");
                    ViewBag.Candidates = await GetActiveCandidates();
                    ViewBag.PollingStations = await GetPollingStations();
                    return View(electionResult);
                }

                electionResult.Verified = false;
                electionResult.CreatedAt = DateTime.UtcNow;
                electionResult.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(electionResult);
                await _context.SaveChangesAsync();
                
                // Récupérer le nom du candidat pour l'affichage
                var candidate = await _context.Candidates.FindAsync(electionResult.CandidateId);
                TempData["Success"] = $"Résultat pour '{candidate?.FirstName + " " + candidate?.LastName ?? "Candidat"}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Candidates = await GetActiveCandidates();
            ViewBag.PollingStations = await GetPollingStations();
            return View(electionResult);
        }

        // GET: ElectionResult/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electionResult = await _context.ElectionResults.FindAsync(id);
            if (electionResult == null)
            {
                return NotFound();
            }
            
            ViewBag.Candidates = await GetActiveCandidates();
            ViewBag.PollingStations = await GetPollingStations();
            return View(electionResult);
        }

        // POST: ElectionResult/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CandidateId,Votes,PollingStationId,Verified,VerificationNotes,CreatedAt")] ElectionResult electionResult)
        {
            if (id != electionResult.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérifier l'unicité (exclure le résultat actuel)
                    var existingResult = await _context.ElectionResults
                        .Where(e => e.Id != id && 
                                   e.CandidateId == electionResult.CandidateId && 
                                   e.PollingStationId == electionResult.PollingStationId)
                        .FirstOrDefaultAsync();

                    if (existingResult != null)
                    {
                        ModelState.AddModelError("", "Un résultat existe déjà pour ce candidat dans ce bureau de vote.");
                        ViewBag.Candidates = await GetActiveCandidates();
                        ViewBag.PollingStations = await GetPollingStations();
                        return View(electionResult);
                    }

                    electionResult.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(electionResult);
                    await _context.SaveChangesAsync();
                    
                    // Récupérer le nom du candidat pour l'affichage
                    var candidate = await _context.Candidates.FindAsync(electionResult.CandidateId);
                    TempData["Success"] = $"Résultat pour '{candidate?.FirstName + " " + candidate?.LastName ?? "Candidat"}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ElectionResultExists(electionResult.Id))
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
            
            ViewBag.Candidates = await GetActiveCandidates();
            ViewBag.PollingStations = await GetPollingStations();
            return View(electionResult);
        }

        // GET: ElectionResult/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electionResult = await _context.ElectionResults
                .Include(e => e.Candidate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (electionResult == null)
            {
                return NotFound();
            }

            return View(electionResult);
        }

        // POST: ElectionResult/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var electionResult = await _context.ElectionResults
                .Include(e => e.Candidate)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (electionResult != null)
            {
                // Vérifier si le résultat est vérifié - protection supplémentaire
                if (electionResult.Verified)
                {
                    TempData["Warning"] = "Attention : Suppression d'un résultat vérifié pour " + electionResult.Candidate.FirstName + " " + electionResult.Candidate.LastName;
                }

                _context.ElectionResults.Remove(electionResult);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Résultat pour '{electionResult.Candidate.FirstName + " " + electionResult.Candidate.LastName}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ElectionResult/VerifyResult/5
        [HttpPost]
        public async Task<IActionResult> VerifyResult(int id, string verifiedBy)
        {
            var electionResult = await _context.ElectionResults.FindAsync(id);
            
            if (electionResult != null)
            {
                electionResult.Verified = true;
                electionResult.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(electionResult);
                await _context.SaveChangesAsync();
                
                // Récupérer le nom du candidat pour l'affichage  
                var candidate = await _context.Candidates.FindAsync(electionResult.CandidateId);
                TempData["Success"] = $"Résultat pour '{candidate?.FirstName + " " + candidate?.LastName ?? "Candidat"}' vérifié avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET ElectionResult/GetByRegion?region=xxx
        [HttpGet]
        public async Task<IActionResult> GetByRegion(string region)
        {
            try
            {
                // Simplification temporaire pour éviter les erreurs de navigation complexes
                var rawResults = await _context.ElectionResults
                    .Include(e => e.Candidate)
                    .Include(e => e.PollingStation)
                    .ToListAsync();

                var results = rawResults
                    .Where(e => region == "all" || true) // Temporairement accepter toutes les régions
                    .Select(e => new
                {
                    id = e.Id,
                    candidateId = e.CandidateId,
                    candidateName = e.Candidate?.FirstName + " " + e.Candidate?.LastName ?? "Candidat inconnu",
                    votes = e.Votes,
                    department = "N/A", // Simplification temporaire
                    commune = "N/A", // Simplification temporaire  
                    pollingStationName = e.PollingStation?.Name ?? "N/A",
                    isVerified = e.Verified,
                    verified = e.Verified
                })
                    .OrderBy(e => e.candidateName)
                    .ThenBy(e => e.department)
                    .ToList();

                return Json(results);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET ElectionResult/GetSummary
        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _context.ElectionResults
                    .Include(e => e.Candidate)
                    .GroupBy(e => e.CandidateId)
                    .Select(g => new
                    {
                        candidateId = g.Key,
                        candidateName = g.First().Candidate.FirstName + " " + g.First().Candidate.LastName,
                        totalVotes = g.Sum(e => e.Votes),
                        verifiedVotes = g.Where(e => e.Verified).Sum(e => e.Votes),
                        pollingStationsCount = g.Count(),
                        verifiedStationsCount = g.Count(e => e.Verified)
                    })
                    .OrderByDescending(s => s.totalVotes)
                    .ToListAsync();

                var totalVotes = summary.Sum(s => s.totalVotes);
                var summaryWithPercentage = summary.Select(s => new
                {
                    s.candidateId,
                    s.candidateName,
                    s.totalVotes,
                    s.verifiedVotes,
                    s.pollingStationsCount,
                    s.verifiedStationsCount,
                    percentage = totalVotes > 0 ? (double)s.totalVotes / totalVotes * 100 : 0
                });

                return Json(summaryWithPercentage);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool ElectionResultExists(int id)
        {
            return _context.ElectionResults.Any(e => e.Id == id);
        }

        private async Task<List<SelectListItem>> GetActiveCandidates()
        {
            return await _context.Candidates
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FirstName + " " + c.LastName + " (" + c.Party + ")"
                })
                .OrderBy(c => c.Text)
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetPollingStations()
        {
            return await _context.PollingStations
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name + " - " + p.Address
                })
                .OrderBy(p => p.Text)
                .ToListAsync();
        }
    }
}