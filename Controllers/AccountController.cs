using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VcBlazor.Services;
using VcBlazor.Data.Models;
using VcBlazor.Data.Models.Authorization;

namespace VcBlazor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IElectionAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IElectionAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _authService.GetUserByEmailAsync(model.Email);
                if (user == null || !await _authService.ValidateUserAsync(model.Email, model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Votre compte est désactivé. Contactez l'administrateur.");
                    return View(model);
                }

                // Créer les claims pour l'utilisateur
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.FullName ?? user.Email ?? "Unknown"),
                    new(ClaimTypes.Email, user.Email ?? ""),
                    new(ClaimTypes.NameIdentifier, user.Id ?? ""),
                    new(ClaimTypes.Role, user.Role ?? ""),
                    new("FirstName", user.FirstName ?? ""),
                    new("LastName", user.LastName ?? ""),
                    new("UserId", user.Id ?? "")
                };

                // Ajouter les permissions comme claims via PermissionRegistry
                var permissions = PermissionRegistry.GetRolePermissions(user.Role ?? "");
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                // Mettre à jour la dernière connexion
                if (int.TryParse(user.Id, out int userId))
                {
                    await _authService.UpdateLastLoginAsync(userId);
                }

                _logger.LogInformation("Connexion réussie pour l'utilisateur: {Email}", model.Email);

                // Vérifier si l'utilisateur doit changer son mot de passe
                if (user.MustChangePassword)
                {
                    return RedirectToAction("ChangePassword", new { mustChange = true });
                }

                // Redirection après connexion
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion pour: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la connexion.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword(bool mustChange = false)
        {
            ViewBag.MustChange = mustChange;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    ModelState.AddModelError(string.Empty, "Session invalide.");
                    return View(model);
                }

                var success = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                if (!success)
                {
                    ModelState.AddModelError(string.Empty, "Mot de passe actuel incorrect.");
                    return View(model);
                }

                TempData["Success"] = "Mot de passe changé avec succès.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe pour l'utilisateur: {UserId}", User.FindFirst("UserId")?.Value);
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors du changement de mot de passe.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync("Cookies");
                _logger.LogInformation("Déconnexion de l'utilisateur: {UserName}", User.Identity?.Name);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToAction("Login");
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'affichage du profil");
                return RedirectToAction("Index", "Home");
            }
        }

#if DEBUG
        /// <summary>
        /// Crée des utilisateurs de démonstration (uniquement en mode DEBUG)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateDemoUsers()
        {
            try
            {
                var demoUsers = new[]
                {
                    new { Email = "superadmin@vcblazor.com", Password = "SuperAdmin123!", Role = ElectionRoles.SuperAdmin, FirstName = "Super", LastName = "Admin" },
                    new { Email = "admin@vcblazor.com", Password = "Admin123!", Role = ElectionRoles.Administrator, FirstName = "Admin", LastName = "System" },
                    new { Email = "supervisor@vcblazor.com", Password = "Super123!", Role = ElectionRoles.Supervisor, FirstName = "Marie", LastName = "Superviseur" },
                    new { Email = "operator@vcblazor.com", Password = "Oper123!", Role = ElectionRoles.Operator, FirstName = "Jean", LastName = "Operateur" },
                    new { Email = "observer@vcblazor.com", Password = "Obs123!", Role = ElectionRoles.Observer, FirstName = "Paul", LastName = "Observateur" }
                };

                int created = 0;
                foreach (var demo in demoUsers)
                {
                    var existingUser = await _authService.GetUserByEmailAsync(demo.Email);
                    if (existingUser == null)
                    {
                        var result = await _authService.CreateUserAsync(demo.Email, demo.Password, demo.FirstName, demo.LastName, demo.Role);
                        if (result != null)
                        {
                            created++;
                        }
                    }
                }

                TempData["Success"] = $"{created} utilisateur(s) de démonstration créé(s). Identifiants disponibles dans les logs.";
                _logger.LogInformation("Utilisateurs créés - SuperAdmin: superadmin@vcblazor.com/SuperAdmin123!, Admin: admin@vcblazor.com/Admin123!, Supervisor: supervisor@vcblazor.com/Super123!, Operator: operator@vcblazor.com/Oper123!, Observer: observer@vcblazor.com/Obs123!");
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création des utilisateurs de démonstration");
                TempData["Error"] = "Erreur lors de la création des utilisateurs de démonstration.";
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Page de création du SuperAdmin initial
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateSuperAdmin()
        {
            return View();
        }

        /// <summary>
        /// Crée le SuperAdmin initial (action sécurisée)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSuperAdmin(string email, string password, string firstName, string lastName, string masterKey)
        {
            try
            {
                // Clé maître pour sécuriser la création du SuperAdmin
                if (masterKey != "VcBlazor2025@MasterKey")
                {
                    TempData["Error"] = "Clé maître invalide.";
                    return RedirectToAction("Login");
                }

                // Vérifier si un SuperAdmin existe déjà
                var existingSuperAdmin = await _authService.GetUserByEmailAsync(email);
                if (existingSuperAdmin != null)
                {
                    TempData["Warning"] = "Un utilisateur avec cet email existe déjà.";
                    return RedirectToAction("Login");
                }

                // Créer le SuperAdmin
                var superAdmin = await _authService.CreateUserAsync(email, password, firstName, lastName, ElectionRoles.SuperAdmin);
                if (superAdmin != null)
                {
                    _logger.LogInformation("SuperAdmin créé avec succès: {Email}", email);
                    TempData["Success"] = "SuperAdmin créé avec succès. Vous pouvez maintenant vous connecter.";
                }
                else
                {
                    TempData["Error"] = "Erreur lors de la création du SuperAdmin.";
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du SuperAdmin");
                TempData["Error"] = "Erreur lors de la création du SuperAdmin.";
                return RedirectToAction("Login");
            }
        }
#endif
    }

    // ViewModels pour les formulaires
    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'email est requis")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Le mot de passe est requis")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Mot de passe actuel")]
        public string CurrentPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères.", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Confirmer le nouveau mot de passe")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}