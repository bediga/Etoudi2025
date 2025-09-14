using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class CommuneController : Controller
    {
        private readonly Vc2025DbContext _context;

        public CommuneController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: Commune
        public async Task<IActionResult> Index()
        {
            var communes = await _context.Communes
                .Include(c => c.Arrondissement)
                    .ThenInclude(a => a.Department)
                        .ThenInclude(d => d.Region)
                .OrderBy(c => c.Arrondissement.Department.Region.Name)
                .ThenBy(c => c.Arrondissement.Department.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(communes);
        }

        // GET: Commune/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commune = await _context.Communes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (commune == null)
            {
                return NotFound();
            }

            return View(commune);
        }

        // GET: Commune/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Arrondissements = await GetArrondissementsList();
            return View();
        }

        // POST: Commune/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Region,Department,Arrondissement,Type,Population,Area,Mayor")] Commune commune)
        {
            if (ModelState.IsValid)
            {
                // Vérifier l'unicité du code dans l'arrondissement
                var existingCommune = await _context.Communes
                    .Where(c => c.Code == commune.Code && c.Arrondissement == commune.Arrondissement)
                    .FirstOrDefaultAsync();

                if (existingCommune != null)
                {
                    ModelState.AddModelError("Code", "Ce code de commune existe déjà dans cet arrondissement.");
                    ViewBag.Arrondissements = await GetArrondissementsList();
                    return View(commune);
                }

                commune.CreatedAt = DateTime.UtcNow;
                commune.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(commune);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Commune '{commune.Name}' créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Arrondissements = await GetArrondissementsList();
            return View(commune);
        }

        // GET: Commune/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commune = await _context.Communes.FindAsync(id);
            if (commune == null)
            {
                return NotFound();
            }
            
            ViewBag.Arrondissements = await GetArrondissementsList();
            return View(commune);
        }

        // POST: Commune/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Region,Department,Arrondissement,Type,Population,Area,Mayor,CreatedAt")] Commune commune)
        {
            if (id != commune.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérifier l'unicité du code (exclure la commune actuelle)
                    var existingCommune = await _context.Communes
                        .Where(c => c.Id != id && c.Code == commune.Code && c.Arrondissement == commune.Arrondissement)
                        .FirstOrDefaultAsync();

                    if (existingCommune != null)
                    {
                        ModelState.AddModelError("Code", "Ce code de commune existe déjà dans cet arrondissement.");
                        ViewBag.Arrondissements = await GetArrondissementsList();
                        return View(commune);
                    }

                    commune.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(commune);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Commune '{commune.Name}' mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommuneExists(commune.Id))
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
            
            ViewBag.Arrondissements = await GetArrondissementsList();
            return View(commune);
        }

        // GET: Commune/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commune = await _context.Communes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (commune == null)
            {
                return NotFound();
            }

            return View(commune);
        }

        // POST: Commune/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commune = await _context.Communes.FindAsync(id);
            
            if (commune != null)
            {
                _context.Communes.Remove(commune);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Commune '{commune.Name}' supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET Commune/GetByArrondissement?arrondissement=xxx
        [HttpGet]
        public async Task<IActionResult> GetByArrondissement(string arrondissement)
        {
            try
            {
                var communes = await _context.Communes
                    .Include(c => c.Arrondissement)
                    .Where(c => c.Arrondissement.Name == arrondissement)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        code = c.Code,
                        arrondissementName = c.Arrondissement.Name
                    })
                    .OrderBy(c => c.name)
                    .ToListAsync();

                return Json(communes);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET Commune/GetByType?type=xxx
        [HttpGet]
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                // L'entité Commune n'a pas de propriété Type, retourner toutes les communes
                var communes = await _context.Communes
                    .Include(c => c.Arrondissement)
                        .ThenInclude(a => a.Department)
                            .ThenInclude(d => d.Region)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        code = c.Code,
                        arrondissementName = c.Arrondissement.Name,
                        departmentName = c.Arrondissement.Department.Name,
                        regionName = c.Arrondissement.Department.Region.Name
                    })
                    .OrderBy(c => c.regionName)
                    .ThenBy(c => c.name)
                    .ToListAsync();

                return Json(communes);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool CommuneExists(int id)
        {
            return _context.Communes.Any(e => e.Id == id);
        }

        private async Task<List<object>> GetArrondissementsList()
        {
            return await _context.Arrondissements
                .Include(a => a.Department)
                    .ThenInclude(d => d.Region)
                .Select(a => new
                {
                    name = a.Name,
                    departmentName = a.Department.Name,
                    regionName = a.Department.Region.Name
                })
                .OrderBy(a => a.regionName)
                .ThenBy(a => a.departmentName)
                .ThenBy(a => a.name)
                .Cast<object>()
                .ToListAsync();
        }
    }
}