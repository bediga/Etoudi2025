using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    [Authorize]
    public class PollingStationController : Controller
    {
        private readonly Vc2025DbContext _context;

        public PollingStationController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: PollingStation
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            // Limiter la taille de page à 20 maximum
            pageSize = Math.Min(pageSize, 20);
            
            var totalCount = await _context.PollingStations.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pollingStations = await _context.PollingStations
                .OrderBy(p => p.Region)
                .ThenBy(p => p.Department)
                .ThenBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            return View(pollingStations);
        }

        // GET: PollingStation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pollingStation = await _context.PollingStations
                .Include(p => p.ElectionResults)
                .Include(p => p.HourlyTurnouts)
                .Include(p => p.Users)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pollingStation == null)
            {
                return NotFound();
            }

            return View(pollingStation);
        }

        // GET: PollingStation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PollingStation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Region,Department,Commune,Arrondissement,Address,RegisteredVoters,Latitude,Longitude,Status,ScrutineersCount,ObserversCount")] PollingStation pollingStation)
        {
            if (ModelState.IsValid)
            {
                pollingStation.CreatedAt = DateTime.UtcNow;
                pollingStation.UpdatedAt = DateTime.UtcNow;
                pollingStation.VotesSubmitted = 0;
                pollingStation.TurnoutRate = 0.0;
                
                _context.Add(pollingStation);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Bureau de vote '{pollingStation.Name}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(pollingStation);
        }

        // GET: PollingStation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pollingStation = await _context.PollingStations.FindAsync(id);
            if (pollingStation == null)
            {
                return NotFound();
            }
            return View(pollingStation);
        }

        // POST: PollingStation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Region,Department,Commune,Arrondissement,Address,RegisteredVoters,Latitude,Longitude,Status,VotesSubmitted,TurnoutRate,ScrutineersCount,ObserversCount,CreatedAt")] PollingStation pollingStation)
        {
            if (id != pollingStation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pollingStation.UpdatedAt = DateTime.UtcNow;
                    pollingStation.LastUpdate = DateTime.UtcNow;
                    
                    // Recalculer le taux de participation
                    if (pollingStation.RegisteredVoters > 0)
                    {
                        pollingStation.TurnoutRate = (double)pollingStation.VotesSubmitted / pollingStation.RegisteredVoters * 100;
                    }
                    
                    _context.Update(pollingStation);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Bureau de vote '{pollingStation.Name}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PollingStationExists(pollingStation.Id))
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
            return View(pollingStation);
        }

        // GET: PollingStation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pollingStation = await _context.PollingStations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pollingStation == null)
            {
                return NotFound();
            }

            return View(pollingStation);
        }

        // POST: PollingStation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pollingStation = await _context.PollingStations.FindAsync(id);
            
            if (pollingStation != null)
            {
                // Vérifier s'il y a des résultats associés
                var resultsCount = await _context.ElectionResults.CountAsync(r => r.PollingStationId == id);
                var usersCount = await _context.Users.CountAsync(u => u.PollingStationId == id);
                
                if (resultsCount > 0 || usersCount > 0)
                {
                    TempData["Error"] = $"Impossible de supprimer le bureau '{pollingStation.Name}'. Il contient des résultats ou des utilisateurs associés.";
                    return RedirectToAction(nameof(Index));
                }

                _context.PollingStations.Remove(pollingStation);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Bureau de vote '{pollingStation.Name}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PollingStation/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var pollingStation = await _context.PollingStations.FindAsync(id);
            
            if (pollingStation != null)
            {
                pollingStation.Status = status;
                pollingStation.UpdatedAt = DateTime.UtcNow;
                pollingStation.LastUpdate = DateTime.UtcNow;
                
                _context.Update(pollingStation);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Statut du bureau '{pollingStation.Name}' mis à jour : {status}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET PollingStation/GetByRegion?region=xxx
        [HttpGet]
        public async Task<IActionResult> GetByRegion(string region)
        {
            try
            {
                var pollingStations = await _context.PollingStations
                    .Where(p => p.Region == region)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        department = p.Department,
                        commune = p.Commune,
                        registeredVoters = p.RegisteredVoters,
                        votesSubmitted = p.VotesSubmitted,
                        turnoutRate = p.TurnoutRate,
                        status = p.Status
                    })
                    .OrderBy(p => p.department)
                    .ThenBy(p => p.name)
                    .ToListAsync();

                return Json(pollingStations);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET PollingStation/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.PollingStations
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        totalStations = g.Count(),
                        openStations = g.Count(p => p.Status == "Ouvert"),
                        closedStations = g.Count(p => p.Status == "Fermé"),
                        totalRegisteredVoters = g.Sum(p => p.RegisteredVoters),
                        totalVotesSubmitted = g.Sum(p => p.VotesSubmitted),
                        overallTurnoutRate = g.Sum(p => p.RegisteredVoters) > 0 ? 
                            (double)g.Sum(p => p.VotesSubmitted) / g.Sum(p => p.RegisteredVoters) * 100 : 0
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalStations = 0,
                    openStations = 0,
                    closedStations = 0,
                    totalRegisteredVoters = 0,
                    totalVotesSubmitted = 0,
                    overallTurnoutRate = 0.0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool PollingStationExists(int id)
        {
            return _context.PollingStations.Any(e => e.Id == id);
        }
    }
}