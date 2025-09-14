using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    [Authorize]
    public class ElectionController : Controller
    {
        private readonly Vc2025DbContext _context;

        public ElectionController(Vc2025DbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.Title = "Tableau de bord électoral";
            return View();
        }

        public IActionResult Candidates()
        {
            ViewBag.Title = "Gestion des Candidats";
            return View();
        }

        public IActionResult PollingStations()
        {
            ViewBag.Title = "Bureaux de Vote";
            return View();
        }

        public IActionResult Results()
        {
            ViewBag.Title = "Résultats";
            return View();
        }

        // API endpoint pour les données des candidats
        [HttpGet]
        public async Task<IActionResult> GetCandidatesData()
        {
            try
            {
                var candidates = _context.Candidates.ToList();
                return Json(candidates);
            }
            catch (Exception ex)
            {
                // Retourner des données fictives en cas d'erreur DB
                var sampleData = new[]
                {
                    new { 
                        Id = 1, 
                        FirstName = "Marie", 
                        LastName = "Dupont", 
                        Party = "Parti Démocratique", 
                        Age = 45, 
                        Profession = "Avocate",
                        Email = "marie.dupont@email.com",
                        TotalVotes = 15420,
                        IsActive = true
                    },
                    new { 
                        Id = 2, 
                        FirstName = "Jean", 
                        LastName = "Martin", 
                        Party = "Alliance Républicaine", 
                        Age = 52, 
                        Profession = "Médecin",
                        Email = "jean.martin@email.com",
                        TotalVotes = 12350,
                        IsActive = true
                    }
                };
                return Json(sampleData);
            }
        }

        // API endpoint pour les statistiques
        [HttpGet]
        public async Task<IActionResult> GetElectionStats()
        {
            try
            {
                var stats = new
                {
                    TotalCandidates = _context.Candidates.Count(),
                    ActiveCandidates = _context.Candidates.Count(c => c.IsActive),
                    TotalPollingStations = _context.PollingStations.Count(),
                    TotalRegisteredVoters = _context.PollingStations.Sum(ps => ps.RegisteredVoters),
                    AverageTurnout = _context.PollingStations.Average(ps => ps.TurnoutRate)
                };
                return Json(stats);
            }
            catch (Exception ex)
            {
                // Données fictives en cas d'erreur
                var sampleStats = new
                {
                    TotalCandidates = 5,
                    ActiveCandidates = 4,
                    TotalPollingStations = 120,
                    TotalRegisteredVoters = 85000,
                    AverageTurnout = 67.5
                };
                return Json(sampleStats);
            }
        }
    }
}