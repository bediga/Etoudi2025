# VcBlazor - Système de Gestion Électorale

Une application ASP.NET Core MVC moderne intégrant des composants Blazor Server avec Syncfusion UI et Entity Framework Core connecté à PostgreSQL.

## 🏗️ Architecture

- **Framework**: .NET 8
- **Web Framework**: ASP.NET Core MVC 8.0
- **UI Components**: Blazor Server + Syncfusion Blazor
- **ORM**: Entity Framework Core 9.0
- **Base de données**: PostgreSQL (via Npgsql)
- **Schéma**: Base Vc2025 (système électoral)

## 📋 Fonctionnalités

### Interface Utilisateur
- **Tableau de bord électoral** avec statistiques en temps réel
- **Gestion des candidats** avec grille Syncfusion interactive
- **Navigation intuitive** avec menu Bootstrap
- **Composants Blazor Server** intégrés dans les vues MVC
- **Interface responsive** avec Bootstrap 5

### Gestion des Données
- **Entités complètes** basées sur le schéma électoral PostgreSQL
- **Hiérarchie administrative** : Régions → Départements → Arrondissements → Communes
- **Infrastructure électorale** : Centres de vote → Bureaux de vote
- **Système de candidats** avec informations détaillées
- **Suivi des résultats** avec vérifications et soumissions
- **Gestion des utilisateurs** et permissions par rôle

## 🚀 Démarrage Rapide

### Prérequis
- .NET 8 SDK ou version ultérieure
- PostgreSQL 12+ (optionnel pour la démo)
- Visual Studio Code avec extensions C#

### Installation

1. **Cloner et naviguer**
   ```bash
   git clone <repository-url>
   cd VCBlazor
   ```

2. **Configuration de la base de données**
   - Modifier la chaîne de connexion dans `appsettings.json`
   - Par défaut : `Host=localhost;Database=Vc2025;Username=postgres;Password=yourpassword`

3. **Lancer l'application**
   ```bash
   dotnet build
   dotnet run
   ```

4. **Accéder à l'application**
   - Ouvrir https://localhost:7156 (ou le port affiché)
   - Naviguer vers le tableau de bord électoral

## 🗄️ Structure du Projet

```
VcBlazor/
├── Controllers/
│   ├── HomeController.cs
│   └── ElectionController.cs          # Contrôleur principal électoral
├── Views/
│   ├── Home/
│   ├── Election/                      # Vues électoral (Dashboard, Candidats)
│   └── Shared/
│       └── _Layout.cshtml            # Layout avec Syncfusion/Bootstrap
├── Components/
│   ├── _Imports.razor                # Directives globales Blazor
│   └── CandidateManagement.razor     # Composant Syncfusion Grid
├── Data/
│   ├── Vc2025DbContext.cs           # Contexte EF Core
│   └── Entities/                     # Entités du modèle électoral
│       ├── Region.cs
│       ├── Department.cs
│       ├── Candidate.cs
│       ├── PollingStation.cs
│       └── ... (20+ entités)
├── wwwroot/                          # Ressources statiques
└── Program.cs                        # Configuration services
```

## 🎯 Pages Principales

### 🏠 Accueil (`/`)
- Présentation du système
- Liens vers les fonctionnalités principales
- Vue d'ensemble des technologies utilisées

### 📊 Tableau de Bord (`/Election/Dashboard`)
- Statistiques en temps réel
- Cartes de métriques colorées
- Actions rapides
- Timeline d'activité récente
- État du système

### 👥 Gestion des Candidats (`/Election/Candidates`)
- **Grille Syncfusion** avec :
  - Pagination, tri, filtrage
  - Colonnes personnalisées
  - Export Excel
- **Statistiques** par parti
- **Données de démonstration** si base non connectée

## 🔧 Configuration

### Base de Données PostgreSQL

La chaîne de connexion par défaut attend :
- **Host**: localhost
- **Database**: Vc2025
- **User**: postgres
- **Password**: yourpassword

Pour modifier, éditer `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=votre-host;Database=Vc2025;Username=user;Password=pass"
  }
}
```

### Mode Sans Base de Données

L'application fonctionne sans PostgreSQL en utilisant des données de démonstration pour :
- Liste des candidats
- Statistiques factices
- Navigation complète

## 📱 Technologies Intégrées

### Syncfusion Blazor
- **SfGrid** : Grilles de données avancées
- **Composants UI** riches et interactifs
- **Thème Bootstrap 5** par défaut
- **Performance** optimisée côté serveur

### Entity Framework Core
- **Code First** avec migrations
- **Relations complexes** entre entités
- **Contraintes** et index configurés
- **Timestamps** automatiques

### Blazor Server
- **Composants** réutilisables
- **Injection de dépendances**
- **Communication** en temps réel via SignalR
- **State management** côté serveur

## 🛠️ Développement

### Extensions VS Code Recommandées
```vscode-extensions
ms-dotnettools.csharp,ms-dotnettools.csdevkit,ckolkman.vscode-postgres
```

### Commandes Utiles

```bash
# Build et run
dotnet build
dotnet run

# Restaurer les packages
dotnet restore

# Créer une migration EF
dotnet ef migrations add InitialCreate

# Appliquer les migrations
dotnet ef database update

# Nettoyer et rebuilder
dotnet clean && dotnet build
```

### Structure des Entités

Le modèle de données comprend :

**Hiérarchie Administrative** (4 niveaux) :
- Regions → Departments → Arrondissements → Communes

**Infrastructure Électorale** :
- VotingCenter (centres de vote)
- PollingStation / PollingStationHierarchy (bureaux)

**Acteurs** :
- Users (utilisateurs système)
- Candidates (candidats électoral)

**Processus Électoral** :
- ResultSubmission (soumissions)
- ElectionResult (résultats)
- VerificationTask (vérifications)

## 📈 Fonctionnalités Avancées

- **API REST** pour données candidates (`/Election/GetCandidatesData`)
- **Statistiques** en temps réel (`/Election/GetElectionStats`)
- **Gestion d'erreurs** gracieuse avec fallback sur données factices
- **Responsive design** pour mobile/tablette
- **Optimisation** des performances avec pagination
- **Sécurisation** des accès base de données

## 🚧 Prochaines Étapes

- [ ] Migrations Entity Framework automatiques
- [ ] Authentification et autorisation
- [ ] API REST complète pour toutes les entités
- [ ] Tests unitaires et d'intégration
- [ ] Déploiement containerisé (Docker)
- [ ] Interface d'administration avancée

## 💡 Utilisation

1. **Démonstration** : Lancer l'application sans base pour voir l'interface
2. **Développement** : Connecter PostgreSQL et importer le schéma fourni
3. **Production** : Configurer les migrations et la sécurité

---

**Développé avec** .NET 8, ASP.NET Core MVC, Blazor Server, Syncfusion, Entity Framework Core et PostgreSQL.