using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VcBlazor.Data.Models.Authorization;
using VcBlazor.Services;

namespace VcBlazor.Attributes
{
    /// <summary>
    /// Attribut d'autorisation personnalisé basé sur les permissions
    /// </summary>
    public class PermissionRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _resource;
        private readonly ActionType _action;
        private readonly PermissionLevel? _minimumLevel;

        /// <summary>
        /// Constructeur pour vérifier une permission spécifique
        /// </summary>
        /// <param name="resource">La ressource (ex: "candidate", "election")</param>
        /// <param name="action">L'action requise</param>
        /// <param name="minimumLevel">Niveau minimum optionnel</param>
        public PermissionRequiredAttribute(string resource, ActionType action, PermissionLevel? minimumLevel = null)
        {
            _resource = resource;
            _action = action;
            _minimumLevel = minimumLevel;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Vérifier si l'utilisateur est authentifié
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Obtenir le service de permissions
            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Vérifier la permission
            var hasPermission = permissionService.CanPerformAction(context.HttpContext.User, _resource, _action);
            
            // Vérifier le niveau minimum si spécifié
            if (hasPermission && _minimumLevel.HasValue)
            {
                hasPermission = permissionService.HasMinimumPermissionLevel(context.HttpContext.User, _minimumLevel.Value);
            }

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    /// <summary>
    /// Attribut pour vérifier plusieurs permissions (OU logique)
    /// </summary>
    public class AnyPermissionRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public AnyPermissionRequiredAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var hasAnyPermission = permissionService.HasAnyPermission(context.HttpContext.User, _permissions);

            if (!hasAnyPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    /// <summary>
    /// Attribut pour vérifier le rôle minimum
    /// </summary>
    public class MinimumRoleRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public MinimumRoleRequiredAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var userRole = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}