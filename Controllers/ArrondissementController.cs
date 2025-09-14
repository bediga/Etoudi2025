using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class ArrondissementController : Controller
    {
        private readonly Vc2025DbContext _context;

        public ArrondissementController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: Arrondissement
        public async Task<IActionResult> Index()
        {
            var arrondissements = await _context.Arrondissements
                .Include(a => a.Department)
                    .ThenInclude(d => d.Region)
                .OrderBy(a => a.Department.Region.Name)
                .ThenBy(a => a.Department.Name)
                .ThenBy(a => a.Name)
                .ToListAsync();
            return View(arrondissements);
        }

        // GET: Arrondissement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrondissement = await _context.Arrondissements
                .FirstOrDefaultAsync(m => m.Id == id);

            if (arrondissement == null)
            {
                return NotFound();
            }

            return View(arrondissement);
        }

        // GET: Arrondissement/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await GetDepartmentsList();
            return View();
        }

        // POST: Arrondissement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Region,Department,Population,Area")] Arrondissement arrondissement)
        {
            if (ModelState.IsValid)
            {
                // Vérifier l'unicité du code dans le département
                var existingArrondissement = await _context.Arrondissements
                    .Where(a => a.Code == arrondissement.Code && a.Department == arrondissement.Department)
                    .FirstOrDefaultAsync();

                if (existingArrondissement != null)
                {
                    ModelState.AddModelError("Code", "Ce code d'arrondissement existe déjà dans ce département.");
                    ViewBag.Departments = await GetDepartmentsList();
                    return View(arrondissement);
                }

                arrondissement.CreatedAt = DateTime.UtcNow;
                arrondissement.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(arrondissement);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Arrondissement '{arrondissement.Name}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Departments = await GetDepartmentsList();
            return View(arrondissement);
        }

        // GET: Arrondissement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrondissement = await _context.Arrondissements.FindAsync(id);
            if (arrondissement == null)
            {
                return NotFound();
            }
            
            ViewBag.Departments = await GetDepartmentsList();
            return View(arrondissement);
        }

        // POST: Arrondissement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Region,Department,Population,Area,CreatedAt")] Arrondissement arrondissement)
        {
            if (id != arrondissement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérifier l'unicité du code (exclure l'arrondissement actuel)
                    var existingArrondissement = await _context.Arrondissements
                        .Where(a => a.Id != id && a.Code == arrondissement.Code && a.Department == arrondissement.Department)
                        .FirstOrDefaultAsync();

                    if (existingArrondissement != null)
                    {
                        ModelState.AddModelError("Code", "Ce code d'arrondissement existe déjà dans ce département.");
                        ViewBag.Departments = await GetDepartmentsList();
                        return View(arrondissement);
                    }

                    arrondissement.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(arrondissement);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Arrondissement '{arrondissement.Name}' mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArrondissementExists(arrondissement.Id))
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
            
            ViewBag.Departments = await GetDepartmentsList();
            return View(arrondissement);
        }

        // GET: Arrondissement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrondissement = await _context.Arrondissements
                .FirstOrDefaultAsync(m => m.Id == id);

            if (arrondissement == null)
            {
                return NotFound();
            }

            return View(arrondissement);
        }

        // POST: Arrondissement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var arrondissement = await _context.Arrondissements.FindAsync(id);
            
            if (arrondissement != null)
            {
                _context.Arrondissements.Remove(arrondissement);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Arrondissement '{arrondissement.Name}' supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET Arrondissement/GetByDepartment?department=xxx
        [HttpGet]
        public async Task<IActionResult> GetByDepartment(string department)
        {
            try
            {
                var arrondissements = await _context.Arrondissements
                    .Include(a => a.Department)
                    .Where(a => a.Department.Name == department)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        code = a.Code,
                        departmentName = a.Department.Name
                    })
                    .OrderBy(a => a.name)
                    .ToListAsync();

                return Json(arrondissements);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool ArrondissementExists(int id)
        {
            return _context.Arrondissements.Any(e => e.Id == id);
        }

        private async Task<List<object>> GetDepartmentsList()
        {
            return await _context.Departments
                .Select(d => new
                {
                    name = d.Name,
                    region = d.Region
                })
                .OrderBy(d => d.region)
                .ThenBy(d => d.name)
                .Cast<object>()
                .ToListAsync();
        }
    }
}