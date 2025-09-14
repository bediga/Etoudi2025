using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class VotingCenterController : Controller
    {
        private readonly Vc2025DbContext _context;

        public VotingCenterController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: VotingCenter
        public async Task<IActionResult> Index()
        {
            try
            {
                var votingCenters = await _context.VotingCenters
                    .Include(v => v.Commune)
                        .ThenInclude(c => c.Arrondissement)
                            .ThenInclude(a => a.Department)
                                .ThenInclude(d => d.Region)
                    .OrderBy(v => v.Name)
                    .ToListAsync();
                return View(votingCenters);
            }
            catch (Exception)
            {
                // Si erreur avec les includes, essayons sans
                var votingCenters = await _context.VotingCenters
                    .ToListAsync();
                return View(votingCenters);
            }
        }

        // GET: VotingCenter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votingCenter = await _context.VotingCenters
                .FirstOrDefaultAsync(m => m.Id == id);

            if (votingCenter == null)
            {
                return NotFound();
            }

            return View(votingCenter);
        }

        // GET: VotingCenter/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsAsync();
            return View();
        }

        // POST: VotingCenter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,CommuneId,Latitude,Longitude,Capacity,PollingStationsCount,Status")] VotingCenter votingCenter)
        {
            if (ModelState.IsValid)
            {
                votingCenter.Status = "active";
                votingCenter.CreatedAt = DateTime.UtcNow;
                votingCenter.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(votingCenter);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Centre de vote '{votingCenter.Name}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            await PopulateViewBagsAsync(votingCenter.CommuneId);
            return View(votingCenter);
        }

        // GET: VotingCenter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votingCenter = await _context.VotingCenters.FindAsync(id);
            if (votingCenter == null)
            {
                return NotFound();
            }
            return View(votingCenter);
        }

        // POST: VotingCenter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,CommuneId,Latitude,Longitude,Capacity,PollingStationsCount,Status,CreatedAt")] VotingCenter votingCenter)
        {
            if (id != votingCenter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    votingCenter.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(votingCenter);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Centre de vote '{votingCenter.Name}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VotingCenterExists(votingCenter.Id))
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
            return View(votingCenter);
        }

        // GET: VotingCenter/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votingCenter = await _context.VotingCenters
                .FirstOrDefaultAsync(m => m.Id == id);

            if (votingCenter == null)
            {
                return NotFound();
            }

            return View(votingCenter);
        }

        // POST: VotingCenter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var votingCenter = await _context.VotingCenters.FindAsync(id);
            
            if (votingCenter != null)
            {
                _context.VotingCenters.Remove(votingCenter);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Centre de vote '{votingCenter.Name}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: VotingCenter/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var votingCenter = await _context.VotingCenters.FindAsync(id);
            
            if (votingCenter != null)
            {
                votingCenter.Status = votingCenter.Status == "active" ? "inactive" : "active";
                votingCenter.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(votingCenter);
                await _context.SaveChangesAsync();
                
                var status = votingCenter.Status == "active" ? "activé" : "désactivé";
                TempData["Success"] = $"Centre de vote '{votingCenter.Name}' {status} avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET VotingCenter/GetByRegion?region=xxx
        [HttpGet]
        public async Task<IActionResult> GetByRegion(string region)
        {
            try
            {
                var votingCenters = await _context.VotingCenters
                    .Include(v => v.Commune)
                    .Where(v => v.Status == "active")
                    .Select(v => new
                    {
                        id = v.Id,
                        name = v.Name,
                        address = v.Address,
                        commune = v.Commune.Name,
                        capacity = v.Capacity,
                        pollingStationsCount = v.PollingStationsCount,
                        status = v.Status
                    })
                    .OrderBy(v => v.commune)
                    .ThenBy(v => v.name)
                    .ToListAsync();

                return Json(votingCenters);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET VotingCenter/GetActive
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var activeVotingCenters = await _context.VotingCenters
                    .Include(v => v.Commune)
                    .Where(v => v.Status == "active")
                    .Select(v => new
                    {
                        id = v.Id,
                        name = v.Name,
                        address = v.Address,
                        commune = v.Commune.Name,
                        capacity = v.Capacity,
                        pollingStationsCount = v.PollingStationsCount,
                        status = v.Status
                    })
                    .OrderBy(v => v.commune)
                    .ThenBy(v => v.name)
                    .ToListAsync();

                return Json(activeVotingCenters);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET VotingCenter/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.VotingCenters
                    .GroupBy(v => 1)
                    .Select(g => new
                    {
                        totalCenters = g.Count(),
                        activeCenters = g.Count(v => v.Status == "active"),
                        inactiveCenters = g.Count(v => v.Status != "active"),
                        totalCapacity = g.Sum(v => v.Capacity),
                        totalPollingStations = g.Sum(v => v.PollingStationsCount)
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalCenters = 0,
                    activeCenters = 0,
                    inactiveCenters = 0,
                    totalCapacity = 0,
                    totalPollingStations = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task PopulateViewBagsAsync(int? selectedCommuneId = null)
        {
            try
            {
                var communes = await _context.Communes
                    .Include(c => c.Arrondissement)
                        .ThenInclude(a => a.Department)
                            .ThenInclude(d => d.Region)
                    .OrderBy(c => c.Name)
                    .Select(c => new { 
                        c.Id, 
                        Name = $"{c.Name} ({c.Arrondissement.Name}, {c.Arrondissement.Department.Name})" 
                    })
                    .ToListAsync();

                ViewBag.CommuneId = new SelectList(communes, "Id", "Name", selectedCommuneId);
            }
            catch (Exception)
            {
                // Si erreur avec les includes, essayons juste les communes
                var communes = await _context.Communes
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.CommuneId = new SelectList(communes, "Id", "Name", selectedCommuneId);
            }
        }

        private bool VotingCenterExists(int id)
        {
            return _context.VotingCenters.Any(e => e.Id == id);
        }
    }
}