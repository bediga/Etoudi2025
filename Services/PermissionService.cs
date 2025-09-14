using System.Security.Claims;
using VcBlazor.Data.Models;
using VcBlazor.Data.Models.Authorization;

namespace VcBlazor.Services
{
    /// <summary>
    /// Service de permissions générique et flexible
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Vérifie si l'utilisateur a une permission spécifique
        /// </summary>
        bool HasPermission(ClaimsPrincipal user, string permissionId);

        /// <summary>
        /// Vérifie si l'utilisateur peut effectuer une action sur une ressource
        /// </summary>
        bool CanPerformAction(ClaimsPrincipal user, string resource, ActionType action);

        /// <summary>
        /// Obtient toutes les permissions de l'utilisateur
        /// </summary>
        IEnumerable<IPermission> GetUserPermissions(ClaimsPrincipal user);

        /// <summary>
        /// Obtient les permissions de l'utilisateur groupées par catégorie
        /// </summary>
        Dictionary<string, IEnumerable<IPermission>> GetUserPermissionsByCategory(ClaimsPrincipal user);

        /// <summary>
        /// Vérifie si l'utilisateur a accès à un niveau de permission minimum
        /// </summary>
        bool HasMinimumPermissionLevel(ClaimsPrincipal user, PermissionLevel minimumLevel);

        /// <summary>
        /// Vérifie les permissions multiples (ET logique)
        /// </summary>
        bool HasAllPermissions(ClaimsPrincipal user, params string[] permissionIds);

        /// <summary>
        /// Vérifie les permissions multiples (OU logique)
        /// </summary>
        bool HasAnyPermission(ClaimsPrincipal user, params string[] permissionIds);
    }

    public class PermissionService : IPermissionService
    {
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ILogger<PermissionService> logger)
        {
            _logger = logger;
        }

        public bool HasPermission(ClaimsPrincipal user, string permissionId)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            try
            {
                // Vérifier via les claims de permission directement
                var hasPermissionClaim = user.HasClaim("permission", permissionId);
                if (hasPermissionClaim)
                    return true;

                // Fallback: vérifier via le rôle
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                    return false;

                return PermissionRegistry.HasPermission(userRole, permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de permission: {PermissionId} pour {User}", 
                    permissionId, user.Identity?.Name);
                return false;
            }
        }

        public bool CanPerformAction(ClaimsPrincipal user, string resource, ActionType action)
        {
            var permissionId = $"{resource.ToLower()}.{action.ToString().ToLower()}";
            return HasPermission(user, permissionId);
        }

        public IEnumerable<IPermission> GetUserPermissions(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return Enumerable.Empty<IPermission>();

            try
            {
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                    return Enumerable.Empty<IPermission>();

                return PermissionRegistry.GetRolePermissionDetails(userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des permissions pour {User}", user.Identity?.Name);
                return Enumerable.Empty<IPermission>();
            }
        }

        public Dictionary<string, IEnumerable<IPermission>> GetUserPermissionsByCategory(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return new Dictionary<string, IEnumerable<IPermission>>();

            try
            {
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                    return new Dictionary<string, IEnumerable<IPermission>>();

                return PermissionRegistry.GetRolePermissionsByCategory(userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des permissions par catégorie pour {User}", user.Identity?.Name);
                return new Dictionary<string, IEnumerable<IPermission>>();
            }
        }

        public bool HasMinimumPermissionLevel(ClaimsPrincipal user, PermissionLevel minimumLevel)
        {
            var userPermissions = GetUserPermissions(user);
            return userPermissions.Any(p => p.Level >= minimumLevel);
        }

        public bool HasAllPermissions(ClaimsPrincipal user, params string[] permissionIds)
        {
            return permissionIds.All(permissionId => HasPermission(user, permissionId));
        }

        public bool HasAnyPermission(ClaimsPrincipal user, params string[] permissionIds)
        {
            return permissionIds.Any(permissionId => HasPermission(user, permissionId));
        }
    }

    /// <summary>
    /// Extensions pour faciliter l'utilisation des permissions
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// Vérifie si l'utilisateur peut lire une ressource
        /// </summary>
        public static bool CanRead(this IPermissionService permissionService, ClaimsPrincipal user, string resource)
        {
            return permissionService.CanPerformAction(user, resource, ActionType.Read);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut créer une ressource
        /// </summary>
        public static bool CanCreate(this IPermissionService permissionService, ClaimsPrincipal user, string resource)
        {
            return permissionService.CanPerformAction(user, resource, ActionType.Create);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut modifier une ressource
        /// </summary>
        public static bool CanUpdate(this IPermissionService permissionService, ClaimsPrincipal user, string resource)
        {
            return permissionService.CanPerformAction(user, resource, ActionType.Update);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut supprimer une ressource
        /// </summary>
        public static bool CanDelete(this IPermissionService permissionService, ClaimsPrincipal user, string resource)
        {
            return permissionService.CanPerformAction(user, resource, ActionType.Delete);
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer une ressource
        /// </summary>
        public static bool CanManage(this IPermissionService permissionService, ClaimsPrincipal user, string resource)
        {
            return permissionService.CanPerformAction(user, resource, ActionType.Manage);
        }

        /// <summary>
        /// Vérifie si l'utilisateur est administrateur
        /// </summary>
        public static bool IsAdmin(this IPermissionService permissionService, ClaimsPrincipal user)
        {
            return user?.IsInRole(ElectionRoles.Administrator) == true;
        }

        /// <summary>
        /// Vérifie si l'utilisateur est superviseur ou plus
        /// </summary>
        public static bool IsSupervisorOrHigher(this IPermissionService permissionService, ClaimsPrincipal user)
        {
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;
            return role == ElectionRoles.Administrator || role == ElectionRoles.Supervisor;
        }
    }
}