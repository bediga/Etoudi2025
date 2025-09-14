using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    [Authorize]
    public class CandidateController : Controller
    {
        private readonly Vc2025DbContext _context;

        public CandidateController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: Candidate
        public async Task<IActionResult> Index()
        {
            var candidates = await _context.Candidates
                .Where(c => c.IsActive)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
            return View(candidates);
        }

        // GET: Candidate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var candidate = await _context.Candidates
                .Include(c => c.ElectionResults)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (candidate == null)
            {
                return NotFound();
            }

            return View(candidate);
        }

        // GET: Candidate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Candidate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Party,Photo,Program,Age,Profession,Education,Experience,Email,Phone,Website,IsActive")] Candidate candidate)
        {
            if (ModelState.IsValid)
            {
                candidate.CreatedAt = DateTime.UtcNow;
                candidate.UpdatedAt = DateTime.UtcNow;
                candidate.TotalVotes = 0;
                
                _context.Add(candidate);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Candidat '{candidate.FirstName} {candidate.LastName}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(candidate);
        }

        // GET: Candidate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }
            return View(candidate);
        }

        // POST: Candidate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Party,Photo,Program,Age,Profession,Education,Experience,Email,Phone,Website,IsActive,TotalVotes,CreatedAt")] Candidate candidate)
        {
            if (id != candidate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    candidate.UpdatedAt = DateTime.UtcNow;
                    _context.Update(candidate);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Candidat '{candidate.FirstName} {candidate.LastName}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CandidateExists(candidate.Id))
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
            return View(candidate);
        }

        // GET: Candidate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(m => m.Id == id);

            if (candidate == null)
            {
                return NotFound();
            }

            return View(candidate);
        }

        // POST: Candidate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            
            if (candidate != null)
            {
                // Vérifier s'il y a des résultats associés
                var resultsCount = await _context.ElectionResults.CountAsync(r => r.CandidateId == id);
                
                if (resultsCount > 0)
                {
                    // Désactiver au lieu de supprimer si il y a des résultats
                    candidate.IsActive = false;
                    candidate.UpdatedAt = DateTime.UtcNow;
                    _context.Update(candidate);
                    
                    TempData["Warning"] = $"Candidat '{candidate.FirstName} {candidate.LastName}' désactivé car il a des résultats associés.";
                }
                else
                {
                    _context.Candidates.Remove(candidate);
                    TempData["Success"] = $"Candidat '{candidate.FirstName} {candidate.LastName}' supprimé avec succès.";
                }
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Candidate/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            
            if (candidate != null)
            {
                candidate.IsActive = !candidate.IsActive;
                candidate.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(candidate);
                await _context.SaveChangesAsync();
                
                var status = candidate.IsActive ? "activé" : "désactivé";
                TempData["Success"] = $"Candidat '{candidate.FirstName} {candidate.LastName}' {status} avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET Candidate/GetCandidates
        [HttpGet]
        public async Task<IActionResult> GetCandidates()
        {
            try
            {
                var candidates = await _context.Candidates
                    .Select(c => new
                    {
                        id = c.Id,
                        firstName = c.FirstName,
                        lastName = c.LastName,
                        fullName = c.FirstName + " " + c.LastName,
                        party = c.Party,
                        age = c.Age,
                        profession = c.Profession,
                        email = c.Email,
                        phone = c.Phone,
                        totalVotes = c.TotalVotes,
                        isActive = c.IsActive,
                        createdAt = c.CreatedAt.ToString("dd/MM/yyyy")
                    })
                    .OrderBy(c => c.lastName)
                    .ThenBy(c => c.firstName)
                    .ToListAsync();

                return Json(candidates);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET Candidate/GetActiveCandidates
        [HttpGet]
        public async Task<IActionResult> GetActiveCandidates()
        {
            try
            {
                var candidates = await _context.Candidates
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        id = c.Id,
                        fullName = c.FirstName + " " + c.LastName,
                        party = c.Party
                    })
                    .OrderBy(c => c.fullName)
                    .ToListAsync();

                return Json(candidates);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool CandidateExists(int id)
        {
            return _context.Candidates.Any(e => e.Id == id);
        }
    }
}