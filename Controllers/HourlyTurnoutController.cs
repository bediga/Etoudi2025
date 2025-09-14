using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class HourlyTurnoutController : Controller
    {
        private readonly Vc2025DbContext _context;

        public HourlyTurnoutController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: HourlyTurnout
        public async Task<IActionResult> Index()
        {
            var hourlyTurnouts = await _context.HourlyTurnouts
                .Include(h => h.PollingStation)
                .OrderByDescending(h => h.RecordedAt)
                .ThenBy(h => h.Hour)
                .ToListAsync();
            return View(hourlyTurnouts);
        }

        // GET: HourlyTurnout/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hourlyTurnout = await _context.HourlyTurnouts
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hourlyTurnout == null)
            {
                return NotFound();
            }

            return View(hourlyTurnout);
        }

        // GET: HourlyTurnout/Create
        public IActionResult Create()
        {
            // Pré-remplir avec l'heure actuelle arrondie
            var model = new HourlyTurnout
            {
                Hour = DateTime.Now.Hour // Hour est un int (0-23)
            };
            return View(model);
        }

        // POST: HourlyTurnout/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,RecordTime,RegisteredVoters,VotesCast,MaleVoters,FemaleVoters,RecordedBy")] HourlyTurnout hourlyTurnout)
        {
            if (ModelState.IsValid)
            {
                // Vérifier qu'il n'y a pas déjà un enregistrement pour cette heure et ce bureau
                var existingRecord = await _context.HourlyTurnouts
                    .Where(h => h.PollingStationId == hourlyTurnout.PollingStationId && 
                               h.Hour == hourlyTurnout.Hour)
                    .FirstOrDefaultAsync();

                if (existingRecord != null)
                {
                    ModelState.AddModelError("Hour", "Un enregistrement existe déjà pour cette heure dans ce bureau de vote.");
                    return View(hourlyTurnout);
                }

                // L'entité HourlyTurnout n'a que les propriétés: Id, PollingStationId, Hour, VotersCount, RecordedAt
                // Pas de propriétés RegisteredVoters, MaleVoters, FemaleVoters, VotesCast, donc on supprime ces validations
                
                // Validation simple sur VotersCount
                if (hourlyTurnout.VotersCount < 0)
                {
                    ModelState.AddModelError("VotersCount", "Le nombre de votants ne peut pas être négatif.");
                    return View(hourlyTurnout);
                }

                // Définir le timestamp d'enregistrement
                hourlyTurnout.RecordedAt = DateTime.UtcNow;
                
                _context.Add(hourlyTurnout);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Participation horaire enregistrée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(hourlyTurnout);
        }

        // GET: HourlyTurnout/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hourlyTurnout = await _context.HourlyTurnouts.FindAsync(id);
            if (hourlyTurnout == null)
            {
                return NotFound();
            }
            
            return View(hourlyTurnout);
        }

        // POST: HourlyTurnout/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,RecordTime,RegisteredVoters,VotesCast,MaleVoters,FemaleVoters,TurnoutRate,RecordedBy,CreatedAt")] HourlyTurnout hourlyTurnout)
        {
            if (id != hourlyTurnout.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validation simple sur VotersCount (seule propriété disponible)
                    if (hourlyTurnout.VotersCount < 0)
                    {
                        ModelState.AddModelError("VotersCount", "Le nombre de votants ne peut pas être négatif.");
                        return View(hourlyTurnout);
                    }

                    // Mettre à jour le timestamp
                    hourlyTurnout.RecordedAt = DateTime.UtcNow;
                    
                    _context.Update(hourlyTurnout);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Participation horaire mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HourlyTurnoutExists(hourlyTurnout.Id))
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
            
            return View(hourlyTurnout);
        }

        // GET: HourlyTurnout/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hourlyTurnout = await _context.HourlyTurnouts
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hourlyTurnout == null)
            {
                return NotFound();
            }

            return View(hourlyTurnout);
        }

        // POST: HourlyTurnout/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hourlyTurnout = await _context.HourlyTurnouts.FindAsync(id);
            
            if (hourlyTurnout != null)
            {
                _context.HourlyTurnouts.Remove(hourlyTurnout);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Enregistrement de participation supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET HourlyTurnout/GetByPollingStation?pollingStationId=xxx
        [HttpGet]
        public async Task<IActionResult> GetByPollingStation(int pollingStationId)
        {
            try
            {
                var turnouts = await _context.HourlyTurnouts
                    .Include(h => h.PollingStation)
                    .Where(h => h.PollingStationId == pollingStationId)
                    .Select(h => new
                    {
                        id = h.Id,
                        hour = h.Hour,
                        votersCount = h.VotersCount,
                        recordedAt = h.RecordedAt,
                        pollingStationName = h.PollingStation.Name
                    })
                    .OrderBy(h => h.hour)
                    .ToListAsync();

                return Json(turnouts);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET HourlyTurnout/GetTurnoutTrends?region=xxx&hours=xxx
        [HttpGet]
        public async Task<IActionResult> GetTurnoutTrends(string region = null, int hours = 24)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-hours);
                var query = _context.HourlyTurnouts.Where(h => h.RecordedAt >= cutoffTime);

                // Note: HourlyTurnout n'a pas de propriété Region directe
                // Il faudrait passer par PollingStation.Commune.Arrondissement.Department.Region si nécessaire
                // Pour l'instant, on ignore le filtre par région

                var trends = await query
                    .GroupBy(h => h.Hour)
                    .Select(g => new
                    {
                        hour = g.Key,
                        totalVoters = g.Sum(h => h.VotersCount),
                        stationsCount = g.Count(),
                        averageVoters = g.Average(h => h.VotersCount)
                    })
                    .OrderBy(t => t.hour)
                    .ToListAsync();

                return Json(trends);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET HourlyTurnout/GetRealTimeStats
        [HttpGet]
        public async Task<IActionResult> GetRealTimeStats()
        {
            try
            {
                var currentHour = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                
                var stats = await _context.HourlyTurnouts
                    .Where(h => h.RecordedAt >= currentHour.AddHours(-1) && h.RecordedAt <= currentHour)
                    .GroupBy(h => 1)
                    .Select(g => new
                    {
                        currentHour = currentHour,
                        totalStationsReporting = g.Count(),
                        totalVotersCount = g.Sum(h => h.VotersCount),
                        averageVotersCount = g.Average(h => h.VotersCount),
                        lastUpdateTime = (DateTime?)g.Max(h => h.RecordedAt)
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    currentHour = currentHour,
                    totalStationsReporting = 0,
                    totalVotersCount = 0,
                    averageVotersCount = 0.0,
                    lastUpdateTime = (DateTime?)null
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool HourlyTurnoutExists(int id)
        {
            return _context.HourlyTurnouts.Any(e => e.Id == id);
        }
    }
}