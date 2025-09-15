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

        public ResultSubmissionController(Vc2025DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté depuis les claims
        /// </summary>
        /// <returns>ID utilisateur connecté ou 1 en mode développement si pas d'authentification</returns>
        private int GetCurrentUserId()
        {
            // Récupérer l'ID utilisateur depuis les claims
            var userIdClaim = User.FindFirst("UserId")?.Value;
            
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }
            
            // Mode développement : utiliser ID 1 par défaut si pas d'authentification
            // TODO: En production, retourner une erreur ou rediriger vers login
            return 1;
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
            // Récupérer tous les candidats depuis la base de données (temporairement sans filtre IsActive)
            var candidates = await _context.Candidates
                .OrderBy(c => c.LastName)
                .ToListAsync();

            // Récupérer les bureaux de vote
            var pollingStations = await _context.PollingStations
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.RegisteredVoters })
                .ToListAsync();

            // Passer les données à la vue
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

        // POST: ResultSubmission/PollingStationSubmit - Traitement soumission bureau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PollingStationSubmit(ResultSubmissionViewModel model)
        {
            try
            {
                Console.WriteLine($"=== DÉBUT SOUMISSION RÉSULTATS ===");
                Console.WriteLine($"Bureau de vote ID: {model.PollingStationId}");
                Console.WriteLine($"Électeurs inscrits: {model.RegisteredVoters}");
                Console.WriteLine($"Total votes: {model.TotalVotes}");
                Console.WriteLine($"Candidats avec résultats: {model.CandidateResults?.Count ?? 0}");
                
                // Validation personnalisée
                if (!model.IsValid(out List<string> validationErrors))
                {
                    Console.WriteLine($"ERREURS DE VALIDATION: {string.Join(", ", validationErrors)}");
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    Console.WriteLine("MODÈLE VALIDE - Utilisation d'ExecutionStrategy SANS transaction manuelle");
                    
                    var strategy = _context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        Console.WriteLine("DÉBUT OPÉRATION AVEC EXECUTION STRATEGY");
                        
                        // 1. Créer ou mettre à jour ResultSubmission
                            Console.WriteLine("1. Recherche soumission existante...");
                        var existingSubmission = await _context.ResultSubmissions
                            .FirstOrDefaultAsync(s => s.PollingStationId == model.PollingStationId);

                        ResultSubmission submission;
                        if (existingSubmission != null)
                        {
                            // Mettre à jour la soumission existante
                            existingSubmission.SubmittedBy = model.SubmittedBy ?? GetCurrentUserId(); // Utilisateur connecté
                            existingSubmission.SubmissionType = model.SubmissionType;
                            existingSubmission.TotalVotes = model.TotalVotes;
                            existingSubmission.RegisteredVoters = model.RegisteredVoters;
                            existingSubmission.TurnoutRate = model.TurnoutRate;
                            existingSubmission.Status = "submitted";
                            existingSubmission.Notes = model.Notes;
                            existingSubmission.SubmittedAt = DateTime.UtcNow;
                            _context.Update(existingSubmission);
                            submission = existingSubmission;
                        }
                        else
                        {
                            // Créer nouvelle soumission
                            submission = new ResultSubmission
                            {
                                PollingStationId = model.PollingStationId,
                                SubmittedBy = model.SubmittedBy ?? GetCurrentUserId(), // Utilisateur connecté
                                SubmissionType = model.SubmissionType,
                                TotalVotes = model.TotalVotes,
                                RegisteredVoters = model.RegisteredVoters,
                                TurnoutRate = model.TurnoutRate,
                                Status = "submitted",
                                Notes = model.Notes,
                                SubmittedAt = DateTime.UtcNow
                            };
                            _context.ResultSubmissions.Add(submission);
                        }

                        await _context.SaveChangesAsync();

                        // 2. Supprimer les anciens détails et créer les nouveaux
                        var existingDetails = await _context.ResultSubmissionDetails
                            .Where(d => d.SubmissionId == submission.Id)
                            .ToListAsync();
                        
                        if (existingDetails.Any())
                        {
                            _context.ResultSubmissionDetails.RemoveRange(existingDetails);
                        }

                        // 3. Créer les nouveaux détails pour chaque candidat
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

                        // 4. Mettre à jour ou créer les résultats globaux (table Results)
                        foreach (var candidateResult in model.CandidateResults.Where(c => c.Votes > 0))
                        {
                            var existingResult = await _context.Results
                                .FirstOrDefaultAsync(r => r.PollingStationId == model.PollingStationId && 
                                                         r.CandidateId == candidateResult.CandidateId);

                            if (existingResult != null)
                            {
                                // Mettre à jour le résultat existant
                                existingResult.Votes = candidateResult.Votes;
                                existingResult.SubmittedBy = model.SubmittedBy; // Permet NULL
                                existingResult.Timestamp = DateTime.UtcNow;
                                existingResult.UpdatedAt = DateTime.UtcNow;
                                existingResult.Verified = false; // Reset verification
                                _context.Update(existingResult);
                            }
                            else
                            {
                                // Créer nouveau résultat
                                var result = new Result
                                {
                                    PollingStationId = model.PollingStationId,
                                    CandidateId = candidateResult.CandidateId,
                                    Votes = candidateResult.Votes,
                                    SubmittedBy = model.SubmittedBy, // Permet NULL
                                    Timestamp = DateTime.UtcNow,
                                    Verified = false,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                _context.Results.Add(result);
                            }
                        }

                        // 5. Mettre à jour ou créer les résultats d'élection globaux (table ElectionResults)
                        foreach (var candidateResult in model.CandidateResults.Where(c => c.Votes > 0))
                        {
                            var existingElectionResult = await _context.ElectionResults
                                .FirstOrDefaultAsync(er => er.CandidateId == candidateResult.CandidateId && 
                                                          er.PollingStationId == model.PollingStationId);

                            if (existingElectionResult != null)
                            {
                                // Mettre à jour le résultat d'élection existant
                                existingElectionResult.Votes = candidateResult.Votes;
                                existingElectionResult.Percentage = candidateResult.Percentage;
                                existingElectionResult.TotalVotes = model.TotalVotes;
                                existingElectionResult.SubmittedAt = DateTime.UtcNow;
                                existingElectionResult.Verified = false; // Reset verification
                                existingElectionResult.UpdatedAt = DateTime.UtcNow;
                                _context.Update(existingElectionResult);
                            }
                            else
                            {
                                // Créer nouveau résultat d'élection
                                var electionResult = new ElectionResult
                                {
                                    CandidateId = candidateResult.CandidateId,
                                    PollingStationId = model.PollingStationId,
                                    Votes = candidateResult.Votes,
                                    Percentage = candidateResult.Percentage,
                                    TotalVotes = model.TotalVotes,
                                    SubmittedAt = DateTime.UtcNow,
                                    Verified = false,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                _context.ElectionResults.Add(electionResult);
                            }
                        }

                        // 6. Supprimer les résultats des candidats avec 0 vote (nettoyage)
                        var candidatesWithZeroVotes = model.CandidateResults.Where(c => c.Votes == 0).Select(c => c.CandidateId).ToList();
                        
                        if (candidatesWithZeroVotes.Any())
                        {
                            // Supprimer de la table Results
                            var resultsToRemove = await _context.Results
                                .Where(r => r.PollingStationId == model.PollingStationId && 
                                           candidatesWithZeroVotes.Contains(r.CandidateId))
                                .ToListAsync();
                            
                            if (resultsToRemove.Any())
                            {
                                _context.Results.RemoveRange(resultsToRemove);
                            }

                            // Supprimer de la table ElectionResults
                            var electionResultsToRemove = await _context.ElectionResults
                                .Where(er => er.PollingStationId == model.PollingStationId && 
                                            candidatesWithZeroVotes.Contains(er.CandidateId))
                                .ToListAsync();
                            
                            if (electionResultsToRemove.Any())
                            {
                                _context.ElectionResults.RemoveRange(electionResultsToRemove);
                            }
                        }

                        Console.WriteLine("SAUVEGARDE FINALE EN COURS...");
                        await _context.SaveChangesAsync();
                        Console.WriteLine("SAUVEGARDE RÉUSSIE!");

                        Console.WriteLine($"=== SOUMISSION TERMINÉE AVEC SUCCÈS ===");
                        TempData["Success"] = $"Résultats soumis avec succès pour le bureau {model.PollingStationId}! Toutes les tables ont été mises à jour.";
                    });

                    return RedirectToAction("Index");
                }

                // Recharger les données pour la vue en cas d'erreur
                await LoadViewDataForSubmission(model);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR GLOBALE SOUMISSION ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"=== FIN ERREUR ===");
                TempData["Error"] = $"Erreur lors de la soumission des résultats : {ex.Message}";
                
                // Recharger les données pour la vue
                await LoadViewDataForSubmission(model);
                return View(model);
            }
        }

        // Méthode helper pour charger les données de la vue
        private async Task LoadViewDataForSubmission(ResultSubmissionViewModel model)
        {
            var candidates = await _context.Candidates
                .OrderBy(c => c.LastName)
                .ToListAsync();

            var pollingStations = await _context.PollingStations
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.RegisteredVoters })
                .ToListAsync();

            ViewBag.Candidates = candidates;
            ViewBag.PollingStations = pollingStations;
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
                        // Créer l'enregistrement de document directement en base pour l'instant
                        var document = new SubmissionDocument
                        {
                            SubmissionId = submissionId,
                            DocumentType = documentType,
                            FileName = Path.GetFileName(file.FileName),
                            OriginalFileName = file.FileName,
                            FilePath = $"uploads/documents/{documentType}/{file.FileName}",
                            FileSize = file.Length,
                            MimeType = file.ContentType,
                            Description = description,
                            IsImage = file.ContentType?.StartsWith("image/") ?? false,
                            UploadedBy = 1, // TODO: Récupérer l'ID utilisateur de la session
                            UploadDate = DateTime.UtcNow,
                            Status = "active"
                        };

                        _context.SubmissionDocuments.Add(document);
                        uploadedDocuments.Add(document);
                    }
                }

                await _context.SaveChangesAsync();

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

                // Récupérer les documents depuis la base de données
                var documents = await _context.SubmissionDocuments
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.SubmissionId == id && d.Status == "active")
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();

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
                var document = await _context.SubmissionDocuments
                    .FirstOrDefaultAsync(d => d.Id == id && d.Status == "active");

                if (document == null)
                {
                    return NotFound("Document introuvable");
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Fichier physique introuvable");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, document.MimeType ?? "application/octet-stream", document.OriginalFileName);
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
                var document = await _context.SubmissionDocuments
                    .FirstOrDefaultAsync(d => d.Id == id && d.Status == "active");

                if (document == null)
                {
                    TempData["Error"] = "Document introuvable.";
                    return RedirectToAction(nameof(DocumentGallery), new { id = submissionId });
                }

                // Supprimer le fichier physique
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Soft delete en base
                document.Status = "deleted";
                document.DeletedAt = DateTime.UtcNow;
                
                _context.Update(document);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Document supprimé avec succès.";
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
                var documents = await _context.SubmissionDocuments
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.SubmissionId == submissionId && d.Status == "active")
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
                
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
                    icon = GetFileIcon(d.MimeType ?? ""),
                    uploadedBy = d.UploadedByUser?.FirstName + " " + d.UploadedByUser?.LastName
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Méthode helper pour obtenir l'icône d'un fichier
        private string GetFileIcon(string mimeType)
        {
            return mimeType switch
            {
                "application/pdf" => "fas fa-file-pdf text-danger",
                var mt when mt.StartsWith("image/") => "fas fa-file-image text-success",
                "application/msword" or 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "fas fa-file-word text-primary",
                "text/plain" => "fas fa-file-alt text-secondary",
                _ => "fas fa-file text-muted"
            };
        }

        // API: Recherche de bureaux de vote (pour autocomplete)
        [HttpGet]
        public async Task<IActionResult> SearchPollingStations(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var searchTerm = term.ToLower().Trim();
            
            var pollingStations = await _context.PollingStations
                .Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Region != null && p.Region.ToLower().Contains(searchTerm)) ||
                    (p.Department != null && p.Department.ToLower().Contains(searchTerm)) ||
                    (p.Commune != null && p.Commune.ToLower().Contains(searchTerm)) ||
                    (p.Arrondissement != null && p.Arrondissement.ToLower().Contains(searchTerm)) ||
                    (p.Address != null && p.Address.ToLower().Contains(searchTerm))
                )
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    registeredVoters = p.RegisteredVoters,
                    fullLocation = (p.Region ?? "") + 
                                 (string.IsNullOrEmpty(p.Region) ? "" : " > ") + (p.Department ?? "") + 
                                 (string.IsNullOrEmpty(p.Department) ? "" : " > ") + (p.Arrondissement ?? "") + 
                                 (string.IsNullOrEmpty(p.Arrondissement) ? "" : " > ") + (p.Commune ?? "") + 
                                 (string.IsNullOrEmpty(p.Commune) ? "" : " > ") + p.Name,
                    address = p.Address ?? "Adresse non spécifiée"
                })
                .OrderBy(p => p.name)
                .Take(20)
                .ToListAsync();

            return Json(pollingStations);
        }

        private bool ResultSubmissionExists(int id)
        {
            return _context.ResultSubmissions.Any(e => e.Id == id);
        }

        // ============ NOUVEAU WORKFLOW EN DEUX ÉTAPES ============

        /// <summary>
        /// GET: Affiche le formulaire de soumission en deux étapes
        /// </summary>
        public async Task<IActionResult> TwoStepSubmission(int? id)
        {
            var model = new TwoStepSubmissionViewModel();

            if (id.HasValue)
            {
                // Charger une soumission existante
                var submission = await _context.ResultSubmissions
                    .Include(r => r.ResultSubmissionDetails)
                    .ThenInclude(rd => rd.Candidate)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (submission != null)
                {
                    model.ResultSubmissionId = submission.Id;
                    model.PollingStationId = submission.PollingStationId;
                    model.TotalVotes = submission.TotalVotes;
                    model.SubmissionType = submission.SubmissionType ?? "final";
                    model.RegisteredVoters = submission.RegisteredVoters;
                    
                    // Récupérer BlankVotes et NullVotes depuis les notes
                    if (!string.IsNullOrEmpty(submission.Notes))
                    {
                        var parts = submission.Notes.Split(';');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("BlankVotes:") && int.TryParse(part.Replace("BlankVotes:", ""), out int blank))
                                model.BlankVotes = blank;
                            if (part.StartsWith("NullVotes:") && int.TryParse(part.Replace("NullVotes:", ""), out int nullVotes))
                                model.NullVotes = nullVotes;
                        }
                    }
                    
                    // Charger les votes des candidats
                    foreach (var detail in submission.ResultSubmissionDetails ?? new List<ResultSubmissionDetail>())
                    {
                        model.CandidateVotes.Add(new CandidateVoteModel
                        {
                            CandidateId = detail.CandidateId,
                            CandidateName = $"{detail.Candidate?.FirstName} {detail.Candidate?.LastName}",
                            Party = detail.Candidate?.Party,
                            Votes = detail.Votes
                        });
                    }

                    // Si on a des votes candidats, on est à l'étape 2
                    if (model.CandidateVotes.Any())
                    {
                        model.CurrentStep = 2;
                    }
                }
            }

            await LoadDropdownData();
            return View(model);
        }

        /// <summary>
        /// POST: Traite l'étape 1 - Bureau de vote et totaux
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitStep1(TwoStepSubmissionViewModel model)
        {
            try
            {
                if (!model.IsStep1Valid)
                {
                    ModelState.AddModelError("", "Veuillez remplir tous les champs requis de l'étape 1.");
                    await LoadDropdownData();
                    return View("TwoStepSubmission", model);
                }

                var currentUserId = GetCurrentUserId();

                // Charger les info du bureau de vote
                var pollingStation = await _context.PollingStations
                    .FirstOrDefaultAsync(p => p.Id == model.PollingStationId);

                if (pollingStation == null)
                {
                    ModelState.AddModelError("PollingStationId", "Bureau de vote introuvable.");
                    await LoadDropdownData();
                    return View("TwoStepSubmission", model);
                }

                // Créer ou mettre à jour la soumission
                ResultSubmission submission;

                if (model.ResultSubmissionId.HasValue)
                {
                    // Mise à jour d'une soumission existante
                    submission = await _context.ResultSubmissions.FindAsync(model.ResultSubmissionId.Value);
                    if (submission == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    // Nouvelle soumission
                    submission = new ResultSubmission();
                    _context.ResultSubmissions.Add(submission);
                }

                // Mettre à jour les données
                submission.PollingStationId = model.PollingStationId.Value;
                submission.TotalVotes = model.TotalVotes;
                submission.SubmissionType = model.SubmissionType;
                submission.RegisteredVoters = model.RegisteredVoters;
                submission.SubmittedBy = currentUserId;
                submission.Status = "draft"; // Brouillon jusqu'à l'étape 2
                submission.SubmittedAt = DateTime.UtcNow;
                
                // Stocker BlankVotes et NullVotes dans les notes pour l'instant
                submission.Notes = $"BlankVotes:{model.BlankVotes};NullVotes:{model.NullVotes}";

                await _context.SaveChangesAsync();

                // Passer à l'étape 2
                model.ResultSubmissionId = submission.Id;
                model.CurrentStep = 2;

                // Charger les candidats disponibles
                await LoadCandidatesForStep2(model);
                await LoadDropdownData();

                TempData["Success"] = "Étape 1 complétée avec succès. Veuillez maintenant saisir les votes par candidat.";
                return View("TwoStepSubmission", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la sauvegarde de l'étape 1 : {ex.Message}";
                await LoadDropdownData();
                return View("TwoStepSubmission", model);
            }
        }

        /// <summary>
        /// POST: Traite l'étape 2 - Votes par candidat
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitStep2(TwoStepSubmissionViewModel model)
        {
            try
            {
                if (!model.IsStep2Valid)
                {
                    ModelState.AddModelError("", "Le total des votes par candidat + votes blancs + votes nuls doit égaler le total des votes.");
                    await LoadCandidatesForStep2(model);
                    await LoadDropdownData();
                    return View("TwoStepSubmission", model);
                }

                if (!model.ResultSubmissionId.HasValue)
                {
                    TempData["Error"] = "Erreur : Aucune soumission trouvée. Veuillez recommencer depuis l'étape 1.";
                    return RedirectToAction("TwoStepSubmission");
                }

                // Charger la soumission existante
                var submission = await _context.ResultSubmissions
                    .Include(r => r.ResultSubmissionDetails)
                    .FirstOrDefaultAsync(r => r.Id == model.ResultSubmissionId.Value);

                if (submission == null)
                {
                    return NotFound();
                }

                // Supprimer les anciens détails
                _context.ResultSubmissionDetails.RemoveRange(submission.ResultSubmissionDetails ?? new List<ResultSubmissionDetail>());

                // Ajouter les nouveaux détails
                foreach (var candidateVote in model.CandidateVotes.Where(c => c.Votes > 0))
                {
                    var detail = new ResultSubmissionDetail
                    {
                        SubmissionId = submission.Id,
                        CandidateId = candidateVote.CandidateId,
                        Votes = candidateVote.Votes,
                        Percentage = (double)candidateVote.Percentage
                    };
                    _context.ResultSubmissionDetails.Add(detail);
                }

                // Finaliser la soumission
                submission.Status = "submitted";

                await _context.SaveChangesAsync();

                TempData["Success"] = "Soumission des résultats terminée avec succès !";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la finalisation : {ex.Message}";
                await LoadCandidatesForStep2(model);
                await LoadDropdownData();
                return View("TwoStepSubmission", model);
            }
        }

        /// <summary>
        /// Charge les candidats disponibles pour l'étape 2
        /// </summary>
        private async Task LoadCandidatesForStep2(TwoStepSubmissionViewModel model)
        {
            try
            {
                var candidates = await _context.Candidates
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();

                // Si pas encore de votes candidats, initialiser avec tous les candidats
                if (!model.CandidateVotes.Any())
                {
                    foreach (var candidate in candidates)
                    {
                        model.CandidateVotes.Add(new CandidateVoteModel
                        {
                            CandidateId = candidate.Id,
                            CandidateName = $"{candidate.FirstName} {candidate.LastName}",
                            Party = candidate.Party,
                            Votes = 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ajouter au moins un candidat vide
                TempData["Warning"] = $"Impossible de charger les candidats : {ex.Message}";
            }
        }

        /// <summary>
        /// Charge les données pour les dropdowns (à implémenter si nécessaire)
        /// </summary>
        private async Task LoadDropdownData()
        {
            // Implémentation future si nécessaire pour les dropdowns
            await Task.CompletedTask;
        }

        /// <summary>
        /// API: Recherche de bureaux de vote pour l'étape 1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchPollingStationsStep1(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new List<PollingStationSimpleModel>());
                }

                var pollingStations = await _context.PollingStations
                    .Where(p => p.Name.Contains(term) || 
                               (p.Commune != null && p.Commune.Contains(term)) ||
                               (p.Region != null && p.Region.Contains(term)) ||
                               (p.Department != null && p.Department.Contains(term)) ||
                               (p.Arrondissement != null && p.Arrondissement.Contains(term)))
                    .Take(10)
                    .Select(p => new PollingStationSimpleModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Region = p.Region,
                        Department = p.Department,
                        Arrondissement = p.Arrondissement,
                        Commune = p.Commune,
                        RegisteredVoters = p.RegisteredVoters
                    })
                    .ToListAsync();

                return Json(pollingStations);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Erreur de recherche : {ex.Message}" });
            }
        }
    }
}
