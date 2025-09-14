using Microsoft.AspNetCore.Identity;
using VcBlazor.Data.Entities;
using VcBlazor.Data.Models;
using VcBlazor.Data.Models.Authorization;
using VcBlazor.Data;
using Microsoft.EntityFrameworkCore;

namespace VcBlazor.Services
{
    /// <summary>
    /// Service d'authentification personnalisé pour le système électoral
    /// Fait le pont entre l'entité User existante et ASP.NET Core Identity
    /// </summary>
    public interface IElectionAuthService
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByIdAsync(int id);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<ApplicationUser> CreateUserAsync(string email, string password, string firstName, string lastName, string role);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IList<ApplicationUser>> GetUsersInRoleAsync(string role);
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        Task<bool> HasPermissionAsync(ApplicationUser user, string permission);
        Task<bool> UpdateLastLoginAsync(int userId);
    }

    public class ElectionAuthService : IElectionAuthService
    {
        private readonly Vc2025DbContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<ElectionAuthService> _logger;

        public ElectionAuthService(
            Vc2025DbContext context, 
            IPasswordHasher<ApplicationUser> passwordHasher,
            ILogger<ElectionAuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                return user != null ? MapToApplicationUser(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par email: {Email}", email);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                return user != null ? MapToApplicationUser(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

                if (user == null) return false;

                var appUser = MapToApplicationUser(user);
                var result = _passwordHasher.VerifyHashedPassword(appUser, user.Password, password);
                
                return result == PasswordVerificationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de l'utilisateur: {Email}", email);
                return false;
            }
        }

        public async Task<ApplicationUser> CreateUserAsync(string email, string password, string firstName, string lastName, string role)
        {
            try
            {
                var appUser = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role
                };

                var hashedPassword = _passwordHasher.HashPassword(appUser, password);

                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = hashedPassword,
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    MustChangePassword = true // Force le changement de mot de passe au premier login
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                appUser.Id = user.Id.ToString();
                return appUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'utilisateur: {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser appUser)
        {
            try
            {
                if (!int.TryParse(appUser.Id, out int userId))
                    return false;

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.FirstName = appUser.FirstName;
                user.LastName = appUser.LastName;
                user.Email = appUser.Email;
                user.Role = appUser.Role;
                user.PollingStationId = appUser.PollingStationId;
                user.Region = appUser.Region;
                user.Department = appUser.Department;
                user.Arrondissement = appUser.Arrondissement;
                user.Commune = appUser.Commune;
                user.PhoneNumber = appUser.PhoneNumber;
                user.IsActive = appUser.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'utilisateur: {UserId}", appUser.Id);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var appUser = MapToApplicationUser(user);
                
                // Vérifier le mot de passe actuel
                var verificationResult = _passwordHasher.VerifyHashedPassword(appUser, user.Password, currentPassword);
                if (verificationResult != PasswordVerificationResult.Success)
                    return false;

                // Hacher le nouveau mot de passe
                user.Password = _passwordHasher.HashPassword(appUser, newPassword);
                user.PasswordChangedAt = DateTime.UtcNow;
                user.MustChangePassword = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe pour l'utilisateur: {UserId}", userId);
                return false;
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string role)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role == role && u.IsActive)
                    .ToListAsync();

                return users.Select(MapToApplicationUser).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs par rôle: {Role}", role);
                return new List<ApplicationUser>();
            }
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return Task.FromResult(user.Role == role);
        }

        public Task<bool> HasPermissionAsync(ApplicationUser user, string permission)
        {
            return Task.FromResult(PermissionRegistry.HasPermission(user.Role ?? "", permission));
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la dernière connexion: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Convertit une entité User en ApplicationUser
        /// </summary>
        private ApplicationUser MapToApplicationUser(User user)
        {
            return new ApplicationUser
            {
                Id = user.Id.ToString(),
                UserName = user.Email,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                PollingStationId = user.PollingStationId,
                AvatarPath = user.AvatarPath,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive,
                Region = user.Region,
                Department = user.Department,
                Arrondissement = user.Arrondissement,
                Commune = user.Commune,
                PhoneNumber = user.PhoneNumber,
                MustChangePassword = user.MustChangePassword,
                LastLoginAt = user.LastLoginAt,
                PasswordChangedAt = user.PasswordChangedAt
            };
        }
    }
}