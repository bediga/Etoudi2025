using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class RegionController : Controller
    {
        private readonly Vc2025DbContext _context;

        public RegionController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: Region
        public async Task<IActionResult> Index()
        {
            var regions = await _context.Regions
                .Include(r => r.Departments)
                .OrderBy(r => r.Name)
                .ToListAsync();
            return View(regions);
        }

        // GET: Region/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .Include(r => r.Departments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // GET: Region/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Region/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code")] Region region)
        {
            if (ModelState.IsValid)
            {
                region.CreatedAt = DateTime.UtcNow;
                region.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(region);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Région '{region.Name}' créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Region/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        // POST: Region/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,CreatedAt")] Region region)
        {
            if (id != region.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    region.UpdatedAt = DateTime.UtcNow;
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Région '{region.Name}' mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.Id))
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
            return View(region);
        }

        // GET: Region/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .Include(r => r.Departments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // POST: Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            
            if (region != null)
            {
                // Vérifier s'il y a des départements associés
                var departmentsCount = await _context.Departments.CountAsync(d => d.RegionId == id);
                
                if (departmentsCount > 0)
                {
                    TempData["Error"] = $"Impossible de supprimer la région '{region.Name}'. Elle contient {departmentsCount} département(s).";
                    return RedirectToAction(nameof(Index));
                }

                _context.Regions.Remove(region);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Région '{region.Name}' supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET Region/GetRegions
        [HttpGet]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var regions = await _context.Regions
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        code = r.Code,
                        departmentsCount = r.Departments.Count(),
                        createdAt = r.CreatedAt.ToString("dd/MM/yyyy"),
                        updatedAt = r.UpdatedAt.ToString("dd/MM/yyyy")
                    })
                    .OrderBy(r => r.name)
                    .ToListAsync();

                return Json(regions);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool RegionExists(int id)
        {
            return _context.Regions.Any(e => e.Id == id);
        }
    }
}