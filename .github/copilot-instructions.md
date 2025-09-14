# Instructions Copilot - VcBlazor

## Contexte du Projet

Ce projet est un **système de gestion électorale** développé en .NET 8 avec :
- **ASP.NET Core MVC** pour l'architecture web
- **Blazor Server** pour les composants interactifs
- **Syncfusion Blazor** pour l'interface utilisateur riche
- **Entity Framework Core** avec **PostgreSQL** (base Vc2025)

## Architecture des Données

### Modèle Electoral PostgreSQL
Le projet utilise un schéma électoral complet avec :
- **Hiérarchie administrative** : Regions → Departments → Arrondissements → Communes
- **Infrastructure électorale** : VotingCenters → PollingStations
- **Acteurs** : Users, Candidates, RolePermissions
- **Processus** : ResultSubmissions, ElectionResults, VerificationTasks
- **Monitoring** : HourlyTurnout, BureauAssignments

### Structure des Entités
Toutes les entités sont dans `Data/Entities/` et suivent les conventions :
- Attributs `[Table]` et `[Column]` pour mapper PostgreSQL
- Navigation properties pour les relations
- Contraintes et validations intégrées
- Timestamps automatiques (created_at, updated_at)

## Composants Techniques

### Blazor + Syncfusion
- **CandidateManagement.razor** : Grille Syncfusion avec données candidats
- **_Imports.razor** : Directives using globales pour Syncfusion/Blazor
- **Intégration MVC** : `Html.RenderComponentAsync()` dans les vues

### Contrôleurs et API
- **ElectionController** : Pages électorales + API REST
- **Endpoints** : `/Election/GetCandidatesData`, `/Election/GetElectionStats`
- **Fallback** : Données de démonstration si base déconnectée

### Configuration
- **Program.cs** : Services Blazor, Syncfusion, EF Core configured
- **appsettings.json** : Chaîne de connexion PostgreSQL
- **_Layout.cshtml** : CSS/JS Syncfusion, Bootstrap, Font Awesome

## Bonnes Pratiques du Projet

### Naming Conventions
- **Entités** : PascalCase, propriétés avec attributs `[Column("snake_case")]`
- **Composants Blazor** : PascalCase.razor
- **Contrôleurs** : Suffixe "Controller"
- **Vues** : Structure MVC standard

### Gestion d'Erreur
- Try-catch dans contrôleurs avec fallback sur données factices
- Logging EF Core activé en développement
- Composants Blazor gèrent exceptions gracieusement

### Performance
- Pagination Syncfusion par défaut (10 éléments)
- Lazy loading EF Core
- Composants Blazor en mode ServerPrerendered

## Instructions de Développement

### Ajout de Nouvelles Fonctionnalités

1. **Nouvelle Entité** :
   - Créer dans `Data/Entities/`
   - Ajouter au DbContext `Vc2025DbContext.cs`
   - Configurer relations dans `OnModelCreating`

2. **Nouveau Composant Blazor** :
   - Créer dans `Components/`
   - Utiliser `@using` directives de `_Imports.razor`
   - Injecter `Vc2025DbContext` si accès données

3. **Nouvelle Page MVC** :
   - Action dans contrôleur approprié
   - Vue dans `Views/{Controller}/`
   - Intégrer composant Blazor si nécessaire

### Base de Données

#### Sans PostgreSQL (Mode Demo)
- Application fonctionne avec données factices
- Parfait pour développement/démonstration
- Pas de dépendance externe

#### Avec PostgreSQL
- Utiliser le schéma SQL fourni dans les entités
- Configurer `appsettings.json`
- Créer migrations : `dotnet ef migrations add InitialCreate`

### Extensions VS Code
Extensions essentielles déjà identifiées :
- **ms-dotnettools.csharp** : Support C# de base
- **ms-dotnettools.csdevkit** : Kit développement C#
- **ckolkman.vscode-postgres** : Gestion PostgreSQL

## Styles et UI

### Thème
- **Bootstrap 5** base
- **Syncfusion Bootstrap theme** pour composants
- **Font Awesome 6** pour icônes
- **Palette** : Primary (bleu), Success (vert), Info (cyan), Warning (orange)

### Composants Syncfusion Utilisés
- **SfGrid** : Grilles de données principales
- **Configurations** : Pagination, tri, filtrage, export Excel
- **Responsive** : Adaptatif mobile/desktop

### Navigation
- **Navbar Bootstrap** avec dropdown Élection
- **Breadcrumbs** pour navigation contextuelle
- **Cards Bootstrap** pour layout dashboard

## Debugging et Tests

### Logs Utiles
- EF Core queries en mode Development
- Erreurs Blazor dans console navigateur
- Exceptions API dans terminal dotnet run

### Points de Contrôle
- Connexion base : Vérifier `GetElectionStats` API
- Composants Blazor : Inspecter avec F12 navigateur  
- Syncfusion : Console pour erreurs JavaScript

## Déploiement

### Développement Local
```bash
dotnet build && dotnet run
```
Accès : https://localhost:7156

### Production Considerations
- Configurer chaîne connexion PostgreSQL sécurisée
- Activer HTTPS
- Optimiser assets Syncfusion (CDN vs local)
- Configurer logging approprié

## Ressources

### Documentation
- [Syncfusion Blazor](https://blazor.syncfusion.com/)
- [EF Core PostgreSQL](https://www.npgsql.org/efcore/)
- [ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/mvc/)

### Patterns du Projet
- **Repository Pattern** : Via DbContext injection
- **MVC + Blazor Hybrid** : Pages MVC avec composants Blazor
- **API First** : Contrôleurs exposent JSON pour flexibilité future