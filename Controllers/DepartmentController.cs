using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly Vc2025DbContext _context;

        public DepartmentController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: Department
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Region)
                .Include(d => d.Arrondissements)
                .OrderBy(d => d.Region.Name)
                .ThenBy(d => d.Name)
                .ToListAsync();
            return View(departments);
        }

        // GET: Department/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Region)
                .Include(d => d.Arrondissements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            ViewData["RegionId"] = new SelectList(_context.Regions.OrderBy(r => r.Name), "Id", "Name");
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,RegionId,Code")] Department department)
        {
            if (ModelState.IsValid)
            {
                department.CreatedAt = DateTime.UtcNow;
                department.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(department);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Département '{department.Name}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["RegionId"] = new SelectList(_context.Regions.OrderBy(r => r.Name), "Id", "Name", department.RegionId);
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            
            ViewData["RegionId"] = new SelectList(_context.Regions.OrderBy(r => r.Name), "Id", "Name", department.RegionId);
            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RegionId,Code,CreatedAt")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    department.UpdatedAt = DateTime.UtcNow;
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Département '{department.Name}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            
            ViewData["RegionId"] = new SelectList(_context.Regions.OrderBy(r => r.Name), "Id", "Name", department.RegionId);
            return View(department);
        }

        // GET: Department/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Region)
                .Include(d => d.Arrondissements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Arrondissements)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (department != null)
            {
                // Vérifier s'il y a des arrondissements associés
                if (department.Arrondissements.Any())
                {
                    TempData["Error"] = $"Impossible de supprimer le département '{department.Name}'. Il contient {department.Arrondissements.Count} arrondissement(s).";
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Département '{department.Name}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET Department/GetByRegion/5
        [HttpGet]
        public async Task<IActionResult> GetByRegion(int regionId)
        {
            try
            {
                var departments = await _context.Departments
                    .Where(d => d.RegionId == regionId)
                    .Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        code = d.Code
                    })
                    .OrderBy(d => d.name)
                    .ToListAsync();

                return Json(departments);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET Department/GetDepartments
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Include(d => d.Region)
                    .Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        code = d.Code,
                        regionName = d.Region.Name,
                        arrondissementsCount = d.Arrondissements.Count(),
                        createdAt = d.CreatedAt.ToString("dd/MM/yyyy"),
                        updatedAt = d.UpdatedAt.ToString("dd/MM/yyyy")
                    })
                    .OrderBy(d => d.regionName)
                    .ThenBy(d => d.name)
                    .ToListAsync();

                return Json(departments);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}