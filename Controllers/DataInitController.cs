using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Controllers
{
    public class DataInitController : Controller
    {
        private readonly Vc2025DbContext _context;

        public DataInitController(Vc2025DbContext context)
        {
            _context = context;
        }

        // GET: DataInit/Initialize
        public async Task<IActionResult> Initialize()
        {
            try
            {
                // Vérifier si des données existent déjà
                var candidateCount = await _context.Candidates.CountAsync();
                var pollingStationCount = await _context.PollingStations.CountAsync();

                if (candidateCount > 0 && pollingStationCount > 0)
                {
                    TempData["Info"] = $"Données déjà présentes : {candidateCount} candidats et {pollingStationCount} bureaux de vote.";
                    return RedirectToAction("Index", "Home");
                }

                // Créer quelques candidats
                if (candidateCount == 0)
                {
                    var candidates = new List<Candidate>
                    {
                        new Candidate
                        {
                            FirstName = "Jean",
                            LastName = "Dupont",
                            Party = "Parti A",
                            Age = 45,
                            Email = "jean.dupont@example.com",
                            Phone = "123456789",
                            Profession = "Avocat",
                            Education = "Master en Droit",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Candidate
                        {
                            FirstName = "Marie",
                            LastName = "Martin",
                            Party = "Parti B",
                            Age = 38,
                            Email = "marie.martin@example.com",
                            Phone = "987654321",
                            Profession = "Enseignante",
                            Education = "Master en Sciences de l'Education",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Candidate
                        {
                            FirstName = "Paul",
                            LastName = "Durand",
                            Party = "Parti C",
                            Age = 52,
                            Email = "paul.durand@example.com",
                            Phone = "456789123",
                            Profession = "Médecin",
                            Education = "Doctorat en Médecine",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Candidates.AddRange(candidates);
                }

                // Créer quelques bureaux de vote
                if (pollingStationCount == 0)
                {
                    var pollingStations = new List<PollingStation>
                    {
                        new PollingStation
                        {
                            Name = "Bureau Centre-ville",
                            Region = "Région Centre",
                            Department = "Département Central",
                            Arrondissement = "1er Arrondissement",
                            Commune = "Commune Centre",
                            Address = "123 Rue Principale",
                            RegisteredVoters = 1500,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new PollingStation
                        {
                            Name = "École Primaire Nord",
                            Region = "Région Nord",
                            Department = "Département Nord",
                            Arrondissement = "2ème Arrondissement",
                            Commune = "Commune Nord",
                            Address = "456 Avenue du Nord",
                            RegisteredVoters = 1200,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new PollingStation
                        {
                            Name = "Salle Communautaire Sud",
                            Region = "Région Sud",
                            Department = "Département Sud",
                            Arrondissement = "3ème Arrondissement",
                            Commune = "Commune Sud",
                            Address = "789 Boulevard du Sud",
                            RegisteredVoters = 980,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new PollingStation
                        {
                            Name = "Lycée de l'Est",
                            Region = "Région Est",
                            Department = "Département Est",
                            Arrondissement = "4ème Arrondissement",
                            Commune = "Commune Est",
                            Address = "321 Rue de l'Est",
                            RegisteredVoters = 1800,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new PollingStation
                        {
                            Name = "Mairie de l'Ouest",
                            Region = "Région Ouest",
                            Department = "Département Ouest",
                            Arrondissement = "5ème Arrondissement",
                            Commune = "Commune Ouest",
                            Address = "654 Place de la Mairie",
                            RegisteredVoters = 1350,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    _context.PollingStations.AddRange(pollingStations);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Base de données initialisée avec succès ! " + 
                    $"{await _context.Candidates.CountAsync()} candidats et " +
                    $"{await _context.PollingStations.CountAsync()} bureaux de vote créés.";

                return RedirectToAction("PollingStationSubmit", "ResultSubmission");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de l'initialisation : {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: DataInit/Status - Vérifier le statut des données
        public async Task<IActionResult> Status()
        {
            try
            {
                var candidateCount = await _context.Candidates.CountAsync();
                var pollingStationCount = await _context.PollingStations.CountAsync();
                var submissionCount = await _context.ResultSubmissions.CountAsync();

                ViewBag.CandidateCount = candidateCount;
                ViewBag.PollingStationCount = pollingStationCount;
                ViewBag.SubmissionCount = submissionCount;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur de connexion à la base : {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}