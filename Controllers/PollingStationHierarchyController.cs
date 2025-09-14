using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class PollingStationHierarchyController : Controller
    {
        private readonly Vc2025DbContext _context;

        public PollingStationHierarchyController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: PollingStationHierarchy
        public async Task<IActionResult> Index()
        {
            try
            {
                var pollingStations = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                        .ThenInclude(v => v.Commune)
                            .ThenInclude(c => c.Arrondissement)
                                .ThenInclude(a => a.Department)
                                    .ThenInclude(d => d.Region)
                    .OrderBy(p => p.VotingCenter.Name)
                    .ThenBy(p => p.StationNumber)
                    .ToListAsync();
                return View(pollingStations);
            }
            catch (Exception)
            {
                // Si erreur avec les includes complexes, essayons une approche plus simple
                var pollingStations = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return View(pollingStations);
            }
        }

        // GET: PollingStationHierarchy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pollingStation = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                        .ThenInclude(v => v.Commune)
                            .ThenInclude(c => c.Arrondissement)
                                .ThenInclude(a => a.Department)
                                    .ThenInclude(d => d.Region)
                    .Include(p => p.BureauAssignments)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pollingStation == null)
                {
                    return NotFound();
                }

                return View(pollingStation);
            }
            catch (Exception)
            {
                var pollingStation = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pollingStation == null)
                {
                    return NotFound();
                }

                return View(pollingStation);
            }
        }

        // GET: PollingStationHierarchy/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsAsync();
            return View();
        }

        // POST: PollingStationHierarchy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,VotingCenterId,StationNumber,RegisteredVoters,VotesSubmitted,TurnoutRate,Status")] PollingStationHierarchy pollingStation)
        {
            if (ModelState.IsValid)
            {
                pollingStation.CreatedAt = DateTime.UtcNow;
                pollingStation.UpdatedAt = DateTime.UtcNow;
                pollingStation.LastUpdate = DateTime.UtcNow;
                
                // Calculer automatiquement le taux de participation si des votes sont soumis
                if (pollingStation.RegisteredVoters > 0)
                {
                    pollingStation.TurnoutRate = (double)pollingStation.VotesSubmitted / pollingStation.RegisteredVoters * 100;
                }
                
                _context.Add(pollingStation);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Bureau de vote '{pollingStation.Name}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            await PopulateViewBagsAsync(pollingStation.VotingCenterId);
            return View(pollingStation);
        }

        // GET: PollingStationHierarchy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pollingStation = await _context.PollingStationsHierarchy.FindAsync(id);
            if (pollingStation == null)
            {
                return NotFound();
            }
            
            await PopulateViewBagsAsync(pollingStation.VotingCenterId);
            return View(pollingStation);
        }

        // POST: PollingStationHierarchy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,VotingCenterId,StationNumber,RegisteredVoters,VotesSubmitted,TurnoutRate,Status,CreatedAt")] PollingStationHierarchy pollingStation)
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
                    if (!PollingStationHierarchyExists(pollingStation.Id))
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
            
            await PopulateViewBagsAsync(pollingStation.VotingCenterId);
            return View(pollingStation);
        }

        // GET: PollingStationHierarchy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pollingStation = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                        .ThenInclude(v => v.Commune)
                    .Include(p => p.BureauAssignments)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pollingStation == null)
                {
                    return NotFound();
                }

                return View(pollingStation);
            }
            catch (Exception)
            {
                var pollingStation = await _context.PollingStationsHierarchy
                    .Include(p => p.VotingCenter)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pollingStation == null)
                {
                    return NotFound();
                }

                return View(pollingStation);
            }
        }

        // POST: PollingStationHierarchy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pollingStation = await _context.PollingStationsHierarchy.FindAsync(id);
            if (pollingStation != null)
            {
                _context.PollingStationsHierarchy.Remove(pollingStation);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Bureau de vote '{pollingStation.Name}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET PollingStationHierarchy/GetStationStats
        [HttpGet]
        public async Task<IActionResult> GetStationStats()
        {
            try
            {
                var stats = await _context.PollingStationsHierarchy
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        totalStations = g.Count(),
                        activeStations = g.Count(p => p.Status == "active"),
                        totalRegisteredVoters = g.Sum(p => p.RegisteredVoters),
                        totalVotesSubmitted = g.Sum(p => p.VotesSubmitted),
                        averageTurnout = g.Average(p => p.TurnoutRate)
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new
                {
                    totalStations = 0,
                    activeStations = 0,
                    totalRegisteredVoters = 0,
                    totalVotesSubmitted = 0,
                    averageTurnout = 0.0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task PopulateViewBagsAsync(int? selectedVotingCenterId = null)
        {
            try
            {
                var votingCenters = await _context.VotingCenters
                    .Include(v => v.Commune)
                        .ThenInclude(c => c.Arrondissement)
                            .ThenInclude(a => a.Department)
                                .ThenInclude(d => d.Region)
                    .OrderBy(v => v.Name)
                    .Select(v => new { 
                        v.Id, 
                        Name = $"{v.Name} ({v.Commune.Name})" 
                    })
                    .ToListAsync();

                ViewBag.VotingCenterId = new SelectList(votingCenters, "Id", "Name", selectedVotingCenterId);
            }
            catch (Exception)
            {
                // Si erreur avec les includes, essayons juste les centres de vote
                var votingCenters = await _context.VotingCenters
                    .OrderBy(v => v.Name)
                    .ToListAsync();

                ViewBag.VotingCenterId = new SelectList(votingCenters, "Id", "Name", selectedVotingCenterId);
            }
        }

        private bool PollingStationHierarchyExists(int id)
        {
            return _context.PollingStationsHierarchy.Any(e => e.Id == id);
        }
    }
}