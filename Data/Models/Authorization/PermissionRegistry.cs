using VcBlazor.Data.Models.Authorization;

namespace VcBlazor.Data.Models.Authorization
{
    /// <summary>
    /// Registre centralisé des permissions du système
    /// Approche générique et extensible
    /// </summary>
    public static class PermissionRegistry
    {
        private static readonly Dictionary<string, IPermission> _permissions = new();
        private static readonly Dictionary<string, string[]> _rolePermissions = new();

        static PermissionRegistry()
        {
            RegisterPermissions();
            RegisterRolePermissions();
        }

        /// <summary>
        /// Enregistre toutes les permissions disponibles dans le système
        /// </summary>
        private static void RegisterPermissions()
        {
            // Permissions Utilisateurs
            RegisterResourcePermissions("Users", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Create, PermissionLevel.High),
                (ActionType.Update, PermissionLevel.Medium),
                (ActionType.Delete, PermissionLevel.High),
                (ActionType.Manage, PermissionLevel.Critical)
            });

            // Permissions Candidats
            RegisterResourcePermissions("Candidates", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Create, PermissionLevel.Medium),
                (ActionType.Update, PermissionLevel.Medium),
                (ActionType.Delete, PermissionLevel.High),
                (ActionType.Manage, PermissionLevel.High)
            });

            // Permissions Résultats
            RegisterResourcePermissions("Results", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Create, PermissionLevel.Medium),
                (ActionType.Update, PermissionLevel.Medium),
                (ActionType.Delete, PermissionLevel.High),
                (ActionType.Verify, PermissionLevel.High),
                (ActionType.Publish, PermissionLevel.Critical),
                (ActionType.Export, PermissionLevel.Medium)
            });

            // Permissions Bureaux de Vote
            RegisterResourcePermissions("PollingStations", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Create, PermissionLevel.Medium),
                (ActionType.Update, PermissionLevel.Medium),
                (ActionType.Delete, PermissionLevel.High),
                (ActionType.Manage, PermissionLevel.High)
            });

            // Permissions Rapports
            RegisterResourcePermissions("Reports", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Export, PermissionLevel.Medium),
                (ActionType.Manage, PermissionLevel.High)
            });

            // Permissions Système
            RegisterResourcePermissions("System", new[]
            {
                (ActionType.Read, PermissionLevel.Medium),
                (ActionType.Admin, PermissionLevel.Critical),
                (ActionType.Manage, PermissionLevel.Critical)
            });

            // Permissions Élections (supervision générale)
            RegisterResourcePermissions("Elections", new[]
            {
                (ActionType.Read, PermissionLevel.Low),
                (ActionType.Manage, PermissionLevel.Critical),
                (ActionType.Verify, PermissionLevel.High)
            });
        }

        /// <summary>
        /// Enregistre les permissions pour une ressource donnée
        /// </summary>
        private static void RegisterResourcePermissions(string resource, (ActionType action, PermissionLevel level)[] actions)
        {
            foreach (var (action, level) in actions)
            {
                var permission = Permission.Create(resource, action, level);
                _permissions[permission.Id] = permission;
            }
        }

        /// <summary>
        /// Configure les permissions par rôle de manière flexible
        /// </summary>
        private static void RegisterRolePermissions()
        {
            // SuperAdmin - Accès total système, toutes permissions y compris gestion des autres admins
            _rolePermissions[ElectionRoles.SuperAdmin] = GetAllPermissions().Select(p => p.Id).ToArray();
            
            // Administrator - Accès complet à tout sauf gestion des SuperAdmin
            _rolePermissions[ElectionRoles.Administrator] = GetAllPermissions()
                .Where(p => p.Level != PermissionLevel.Critical || !p.Id.Contains("superadmin"))
                .Select(p => p.Id).ToArray();

            // Supervisor - Supervision et vérification
            _rolePermissions[ElectionRoles.Supervisor] = GetPermissionsByLevel(PermissionLevel.Low, PermissionLevel.Medium)
                .Concat(GetPermissionsByAction(ActionType.Verify))
                .Concat(GetPermissionsByResource("Reports"))
                .Select(p => p.Id)
                .Distinct()
                .ToArray();

            // Operator - Opérations courantes (pas de suppression ni administration)
            _rolePermissions[ElectionRoles.Operator] = GetPermissionsByAction(ActionType.Read, ActionType.Create, ActionType.Update)
                .Where(p => p.Level <= PermissionLevel.Medium)
                .Select(p => p.Id)
                .ToArray();

            // PresidentBureau - Gestion spécifique de son bureau
            _rolePermissions[ElectionRoles.PresidentBureau] = new[]
            {
                "candidates.read", "results.read", "results.create", "results.update",
                "pollingstations.read", "pollingstations.update", "reports.read"
            };

            // Assesseur - Assistance au bureau
            _rolePermissions[ElectionRoles.Assesseur] = new[]
            {
                "candidates.read", "results.read", "results.create",
                "pollingstations.read"
            };

            // Observer - Lecture seule
            _rolePermissions[ElectionRoles.Observer] = GetPermissionsByAction(ActionType.Read)
                .Select(p => p.Id)
                .ToArray();
        }

        /// <summary>
        /// Obtient toutes les permissions enregistrées
        /// </summary>
        public static IEnumerable<IPermission> GetAllPermissions()
        {
            return _permissions.Values;
        }

        /// <summary>
        /// Obtient une permission par son ID
        /// </summary>
        public static IPermission? GetPermission(string permissionId)
        {
            return _permissions.TryGetValue(permissionId, out var permission) ? permission : null;
        }

        /// <summary>
        /// Obtient les permissions par niveau de sécurité
        /// </summary>
        public static IEnumerable<IPermission> GetPermissionsByLevel(params PermissionLevel[] levels)
        {
            return _permissions.Values.Where(p => levels.Contains(p.Level));
        }

        /// <summary>
        /// Obtient les permissions par type d'action
        /// </summary>
        public static IEnumerable<IPermission> GetPermissionsByAction(params ActionType[] actions)
        {
            var actionStrings = actions.Select(a => a.ToString().ToLower());
            return _permissions.Values.Where(p => actionStrings.Any(a => p.Id.EndsWith(a)));
        }

        /// <summary>
        /// Obtient les permissions pour une ressource spécifique
        /// </summary>
        public static IEnumerable<IPermission> GetPermissionsByResource(string resource)
        {
            var prefix = resource.ToLower() + ".";
            return _permissions.Values.Where(p => p.Id.StartsWith(prefix));
        }

        /// <summary>
        /// Obtient les permissions pour un rôle
        /// </summary>
        public static string[] GetRolePermissions(string role)
        {
            return _rolePermissions.TryGetValue(role, out var permissions) ? permissions : Array.Empty<string>();
        }

        /// <summary>
        /// Vérifie si un rôle a une permission spécifique
        /// </summary>
        public static bool HasPermission(string role, string permissionId)
        {
            return _rolePermissions.TryGetValue(role, out var permissions) && 
                   permissions.Contains(permissionId);
        }

        /// <summary>
        /// Obtient les permissions d'un rôle avec leurs détails
        /// </summary>
        public static IEnumerable<IPermission> GetRolePermissionDetails(string role)
        {
            var permissionIds = GetRolePermissions(role);
            return permissionIds.Select(id => GetPermission(id)).Where(p => p != null).Cast<IPermission>();
        }

        /// <summary>
        /// Obtient les permissions groupées par catégorie pour un rôle
        /// </summary>
        public static Dictionary<string, IEnumerable<IPermission>> GetRolePermissionsByCategory(string role)
        {
            return GetRolePermissionDetails(role)
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Ajoute dynamiquement une nouvelle permission
        /// </summary>
        public static void RegisterPermission(IPermission permission)
        {
            _permissions[permission.Id] = permission;
        }

        /// <summary>
        /// Ajoute une permission à un rôle
        /// </summary>
        public static void AddPermissionToRole(string role, string permissionId)
        {
            if (!_rolePermissions.ContainsKey(role))
            {
                _rolePermissions[role] = Array.Empty<string>();
            }

            var currentPermissions = _rolePermissions[role].ToList();
            if (!currentPermissions.Contains(permissionId))
            {
                currentPermissions.Add(permissionId);
                _rolePermissions[role] = currentPermissions.ToArray();
            }
        }

        /// <summary>
        /// Retire une permission d'un rôle
        /// </summary>
        public static void RemovePermissionFromRole(string role, string permissionId)
        {
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                _rolePermissions[role] = permissions.Where(p => p != permissionId).ToArray();
            }
        }
    }
}