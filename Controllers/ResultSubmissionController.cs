using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Data.Entities;
using VcBlazor.Models;
using VcBlazor.Services;

namespace VcBlazor.Controllers
{
    public class ResultSubmissionController : Controller
    {
        private readonly Vc2025DbContext _context;
        private readonly IDocumentService _documentService;

        public ResultSubmissionController(Vc2025DbContext context, IDocumentService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        // GET: ResultSubmission
        public async Task<IActionResult> Index()
        {
            var resultSubmissions = await _context.ResultSubmissions
                .Include(r => r.ResultSubmissionDetails)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
            return View(resultSubmissions);
        }

        // GET: ResultSubmission/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions
                .Include(r => r.ResultSubmissionDetails)
                .Include(r => r.SubmissionDocuments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resultSubmission == null)
            {
                return NotFound();
            }

            return View(resultSubmission);
        }

        // GET: ResultSubmission/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: ResultSubmission/PollingStationSubmit - Nouvelle action pour soumission par bureau
        public async Task<IActionResult> PollingStationSubmit(int? pollingStationId)
        {
            try
            {
                // Récupérer les candidats actifs
                var candidates = await _context.Candidates
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.LastName)
                    .ToListAsync();

                // Récupérer les bureaux de vote si pas spécifié
                var pollingStations = await _context.PollingStations
                    .OrderBy(p => p.Name)
                    .Select(p => new { p.Id, p.Name, p.RegisteredVoters })
                    .ToListAsync();

                ViewBag.Candidates = candidates;
                ViewBag.PollingStations = pollingStations;
                ViewBag.SelectedPollingStationId = pollingStationId;

                var model = new ResultSubmissionViewModel
                {
                    PollingStationId = pollingStationId ?? 0,
                    SubmissionType = "final",
                    Status = "pending"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Mode démo si base non disponible
                Console.WriteLine($"Erreur base de données : {ex.Message}");
                
                var demoCandidates = new[]
                {
                    new { Id = 1, FirstName = "Paul", LastName = "BIYA", Party = "RDPC" },
                    new { Id = 2, FirstName = "Maurice", LastName = "KAMTO", Party = "MRC" },
                    new { Id = 3, FirstName = "Cabral", LastName = "LIBII", Party = "PURS" }
                };

                var demoPollingStations = new[]
                {
                    new { Id = 1, Name = "Bureau Central Yaoundé", RegisteredVoters = 1200 },
                    new { Id = 2, Name = "École Publique Mendong", RegisteredVoters = 800 },
                    new { Id = 3, Name = "Lycée Bilingue Douala", RegisteredVoters = 950 }
                };

                ViewBag.Candidates = demoCandidates;
                ViewBag.PollingStations = demoPollingStations;
                ViewBag.SelectedPollingStationId = pollingStationId;

                var model = new ResultSubmissionViewModel
                {
                    PollingStationId = pollingStationId ?? 1,
                    SubmissionType = "final",
                    Status = "pending"
                };

                return View(model);
            }
        }

        // POST: ResultSubmission/PollingStationSubmit - Traitement soumission bureau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PollingStationSubmit(ResultSubmissionViewModel model)
        {
            try
            {
                // Validation personnalisée
                if (!model.IsValid(out List<string> validationErrors))
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // Créer l'entité ResultSubmission
                    var submission = new ResultSubmission
                    {
                        PollingStationId = model.PollingStationId,
                        SubmittedBy = model.SubmittedBy ?? 1, // TODO: Récupérer de la session utilisateur
                        SubmissionType = model.SubmissionType,
                        TotalVotes = model.TotalVotes,
                        RegisteredVoters = model.RegisteredVoters,
                        TurnoutRate = model.TurnoutRate,
                        Status = "submitted",
                        Notes = model.Notes,
                        SubmittedAt = DateTime.UtcNow
                    };

                    _context.ResultSubmissions.Add(submission);
                    await _context.SaveChangesAsync();

                    // Créer les détails pour chaque candidat
                    foreach (var candidateResult in model.CandidateResults.Where(c => c.Votes > 0))
                    {
                        var detail = new ResultSubmissionDetail
                        {
                            SubmissionId = submission.Id,
                            CandidateId = candidateResult.CandidateId,
                            Votes = candidateResult.Votes,
                            Percentage = candidateResult.Percentage
                        };

                        _context.ResultSubmissionDetails.Add(detail);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Résultats soumis avec succès pour le bureau {model.PollingStationId}!";
                    return RedirectToAction("Details", new { id = submission.Id });
                }

                // Recharger les données pour la vue en cas d'erreur
                await LoadViewDataForSubmission(model);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la soumission : {ex.Message}");
                TempData["Error"] = "Erreur lors de la soumission. Mode démo activé.";
                
                // En mode démo, simuler succès
                TempData["Success"] = $"[DEMO] Résultats soumis avec succès pour le bureau {model.PollingStationId}!";
                return RedirectToAction("Index");
            }
        }

        // Méthode helper pour charger les données de la vue
        private async Task LoadViewDataForSubmission(ResultSubmissionViewModel model)
        {
            try
            {
                var candidates = await _context.Candidates
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.LastName)
                    .ToListAsync();

                var pollingStations = await _context.PollingStations
                    .OrderBy(p => p.Name)
                    .Select(p => new { p.Id, p.Name, p.RegisteredVoters })
                    .ToListAsync();

                ViewBag.Candidates = candidates;
                ViewBag.PollingStations = pollingStations;
            }
            catch (Exception ex)
            {
                // Afficher l'erreur à l'utilisateur
                Console.WriteLine($"Erreur de connexion à la base de données : {ex.Message}");
                TempData["DatabaseError"] = $"Impossible de se connecter à la base de données : {ex.Message}";
                
                // Utiliser des données vides pour éviter les erreurs de vue
                ViewBag.Candidates = new object[0];
                ViewBag.PollingStations = new object[0];
            }
        }

        // POST: ResultSubmission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,SubmittedBy,VotersCount,VotesCount,BlankVotes,InvalidVotes,ValidVotes,Observations")] ResultSubmission resultSubmission)
        {
            if (ModelState.IsValid)
            {
                // Validation simple : les votes totaux ne peuvent pas dépasser les électeurs inscrits  
                if (resultSubmission.TotalVotes > resultSubmission.RegisteredVoters)
                {
                    ModelState.AddModelError("VotesCount", 
                        "Le nombre de votes ne peut pas dépasser le nombre d'électeurs inscrits.");
                    return View(resultSubmission);
                }

                resultSubmission.Status = "submitted";
                resultSubmission.SubmittedAt = DateTime.UtcNow;
                
                _context.Add(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Soumission de résultats créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(resultSubmission);
        }

        // GET: ResultSubmission/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            if (resultSubmission == null)
            {
                return NotFound();
            }

            // Ne permettre la modification que si pas encore validé
            if (resultSubmission.Status == "Validé")
            {
                TempData["Error"] = "Impossible de modifier une soumission déjà validée.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            return View(resultSubmission);
        }

        // POST: ResultSubmission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PollingStationId,PollingStationName,Region,Department,Commune,Arrondissement,SubmittedBy,VotersCount,VotesCount,BlankVotes,InvalidVotes,ValidVotes,Observations,Status,SubmittedAt,CreatedAt")] ResultSubmission resultSubmission)
        {
            if (id != resultSubmission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vérification simple
                    if (resultSubmission.TotalVotes > resultSubmission.RegisteredVoters)
                    {
                        ModelState.AddModelError("TotalVotes", 
                            "Le nombre de votes ne peut pas dépasser le nombre d'électeurs inscrits.");
                        return View(resultSubmission);
                    }
                    
                    _context.Update(resultSubmission);
                    await _context.SaveChangesAsync();
                    
                                        TempData["Success"] = "Soumission de résultats mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResultSubmissionExists(resultSubmission.Id))
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
            
            return View(resultSubmission);
        }

        // GET: ResultSubmission/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resultSubmission = await _context.ResultSubmissions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resultSubmission == null)
            {
                return NotFound();
            }

            return View(resultSubmission);
        }

        // POST: ResultSubmission/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            
            if (resultSubmission != null)
            {
                // Vérifier si la soumission est validée
                if (resultSubmission.Status == "Validé")
                {
                    TempData["Error"] = $"Impossible de supprimer la soumission pour '{resultSubmission.PollingStation?.Name ?? "N/A"}'. Elle est déjà validée.";
                    return RedirectToAction(nameof(Index));
                }

                _context.ResultSubmissions.Remove(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Soumission pour '{resultSubmission.PollingStation?.Name ?? "N/A"}' supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ResultSubmission/ChangeStatus/5
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus, string? reviewedBy = null)
        {
            var resultSubmission = await _context.ResultSubmissions.FindAsync(id);
            
            if (resultSubmission != null)
            {
                var oldStatus = resultSubmission.Status;
                resultSubmission.Status = newStatus;

                if (newStatus == "verified")
                {
                    if (int.TryParse(reviewedBy, out int userId))
                    {
                        resultSubmission.VerifiedBy = userId;
                    }
                    resultSubmission.VerifiedAt = DateTime.UtcNow;
                }
                else if (newStatus == "Rejeté")
                {
                    if (int.TryParse(reviewedBy, out int userId))
                    {
                        resultSubmission.VerifiedBy = userId;
                    }
                    resultSubmission.VerifiedAt = DateTime.UtcNow;
                }
                
                _context.Update(resultSubmission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Statut de la soumission '{resultSubmission.PollingStation?.Name ?? "N/A"}' changé de '{oldStatus}' à '{newStatus}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET ResultSubmission/GetByStatus?status=xxx
        [HttpGet]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                // Simplification temporaire pour éviter les erreurs de navigation complexes
                var rawSubmissions = await _context.ResultSubmissions
                    .Include(r => r.PollingStation)
                    .Where(r => r.Status == status)
                    .ToListAsync();

                var submissions = rawSubmissions.Select(r => new
                {
                    id = r.Id,
                    pollingStationName = r.PollingStation?.Name ?? "N/A",
                    region = "N/A", // Simplification temporaire
                    department = "N/A", // Simplification temporaire
                    commune = "N/A", // Simplification temporaire
                    submittedBy = r.SubmittedBy,
                    submittedAt = r.SubmittedAt,
                    votersCount = r.RegisteredVoters,
                    votesCount = r.TotalVotes,
                    validVotes = r.TotalVotes,
                    reviewedBy = r.VerifiedBy,
                    reviewedAt = r.VerifiedAt
                })
                .OrderByDescending(r => r.submittedAt)
                .ToList();

                return Json(submissions);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: GET ResultSubmission/GetStats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _context.ResultSubmissions
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        totalSubmissions = g.Count(),
                        submittedCount = g.Count(r => r.Status == "Soumis"),
                        validatedCount = g.Count(r => r.Status == "Validé"),
                        rejectedCount = g.Count(r => r.Status == "Rejeté"),
                        totalVoters = g.Sum(r => r.RegisteredVoters),
                        totalVotes = g.Sum(r => r.TotalVotes),
                        totalValidVotes = g.Sum(r => r.TotalVotes),
                        overallTurnout = g.Sum(r => r.RegisteredVoters) > 0 ? 
                            (double)g.Sum(r => r.TotalVotes) / g.Sum(r => r.RegisteredVoters) * 100 : 0
                    })
                    .FirstOrDefaultAsync();

                return Json(stats ?? new 
                {
                    totalSubmissions = 0,
                    submittedCount = 0,
                    validatedCount = 0,
                    rejectedCount = 0,
                    totalVoters = 0,
                    totalVotes = 0,
                    totalValidVotes = 0,
                    overallTurnout = 0.0
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // === GESTION DES DOCUMENTS ===

        // GET: ResultSubmission/UploadDocument/5
        public async Task<IActionResult> UploadDocument(int id)
        {
            var submission = await _context.ResultSubmissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            ViewBag.SubmissionId = id;
            ViewBag.Submission = submission;
            return View();
        }

        // POST: ResultSubmission/UploadDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int submissionId, IFormFile[] files, string[] documentTypes, string[] descriptions)
        {
            try
            {
                if (files == null || files.Length == 0)
                {
                    TempData["Error"] = "Aucun fichier sélectionné.";
                    return RedirectToAction(nameof(UploadDocument), new { id = submissionId });
                }

                var uploadedDocuments = new List<SubmissionDocument>();

                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var documentType = i < documentTypes.Length ? documentTypes[i] : "document";
                    var description = i < descriptions.Length ? descriptions[i] : null;

                    if (file.Length > 0)
                    {
                        var document = await _documentService.CreateDocumentRecordAsync(
                            submissionId, 
                            file, 
                            documentType, 
                            description, 
                            1 // TODO: Récupérer l'ID utilisateur de la session
                        );

                        uploadedDocuments.Add(document);
                    }
                }

                TempData["Success"] = $"{uploadedDocuments.Count} document(s) téléchargé(s) avec succès.";
                return RedirectToAction(nameof(Details), new { id = submissionId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du téléchargement : {ex.Message}";
                return RedirectToAction(nameof(UploadDocument), new { id = submissionId });
            }
        }

        // GET: ResultSubmission/DocumentGallery/5
        public async Task<IActionResult> DocumentGallery(int id)
        {
            try
            {
                var submission = await _context.ResultSubmissions
                    .Include(s => s.PollingStation)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (submission == null)
                {
                    return NotFound();
                }

                var documents = await _documentService.GetSubmissionDocumentsAsync(id);

                ViewBag.Submission = submission;
                return View(documents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des documents : {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: ResultSubmission/DownloadDocument/5
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var fileResult = await _documentService.GetDocumentContentAsync(id);
                return File(fileResult.Content, fileResult.MimeType, fileResult.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du téléchargement : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ResultSubmission/DeleteDocument/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id, int submissionId)
        {
            try
            {
                var success = await _documentService.DeleteDocumentAsync(id);
                
                if (success)
                {
                    TempData["Success"] = "Document supprimé avec succès.";
                }
                else
                {
                    TempData["Error"] = "Impossible de supprimer le document.";
                }

                return RedirectToAction(nameof(DocumentGallery), new { id = submissionId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
                return RedirectToAction(nameof(DocumentGallery), new { id = submissionId });
            }
        }

        // API: Récupérer les documents d'une soumission (pour AJAX)
        [HttpGet]
        public async Task<IActionResult> GetDocuments(int submissionId)
        {
            try
            {
                var documents = await _documentService.GetSubmissionDocumentsAsync(submissionId);
                
                var result = documents.Select(d => new 
                {
                    id = d.Id,
                    fileName = d.FileName,
                    documentType = d.DocumentType,
                    description = d.Description,
                    fileSize = d.FileSize,
                    uploadDate = d.UploadDate.ToString("dd/MM/yyyy HH:mm"),
                    isImage = d.IsImage,
                    mimeType = d.MimeType,
                    icon = _documentService.GetFileIcon(d.MimeType ?? ""),
                    uploadedBy = d.UploadedByUser?.FirstName + " " + d.UploadedByUser?.LastName
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool ResultSubmissionExists(int id)
        {
            return _context.ResultSubmissions.Any(e => e.Id == id);
        }
    }
}
