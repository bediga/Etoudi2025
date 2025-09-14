# Configuration de la Base de Données PostgreSQL - VcBlazor

## 📋 Instructions de Configuration

### Option 1: PostgreSQL Local (Recommandé)

#### 1. Installation PostgreSQL
```bash
# Windows - via Chocolatey
choco install postgresql

# Ou télécharger depuis: https://www.postgresql.org/download/windows/
```

#### 2. Configuration de la Base
```sql
-- Connectez-vous comme administrateur PostgreSQL
psql -U postgres

-- Créer la base de données de développement
CREATE DATABASE "Vc2025_Dev";

-- Créer la base de données de production (optionnel)
CREATE DATABASE "Vc2025";

-- Créer un utilisateur spécifique (optionnel mais recommandé)
CREATE USER vcblazor WITH ENCRYPTED PASSWORD 'VcBlazor2025!';
GRANT ALL PRIVILEGES ON DATABASE "Vc2025_Dev" TO vcblazor;
GRANT ALL PRIVILEGES ON DATABASE "Vc2025" TO vcblazor;

-- Quitter psql
\q
```

#### 3. Modifier appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=Vc2025_Dev;Username=vcblazor;Password=VcBlazor2025!;Port=5432;Pooling=true;"
  }
}
```

### Option 2: PostgreSQL avec Docker (Alternative)

#### 1. Créer docker-compose.yml
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    container_name: vcblazor_postgres
    environment:
      POSTGRES_DB: Vc2025_Dev
      POSTGRES_USER: vcblazor  
      POSTGRES_PASSWORD: VcBlazor2025!
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

#### 2. Lancer PostgreSQL
```bash
docker-compose up -d
```

### Option 3: Mode Démo Sans Base (Déjà Configuré)

L'application fonctionne **automatiquement** sans PostgreSQL avec des données factices.
Idéal pour développement et démonstration !

## 🔧 Migration des Données

Une fois PostgreSQL configuré :

```bash
# Appliquer la migration
dotnet ef database update

# Vérifier les tables créées
dotnet ef migrations list
```

## 🗄️ Structure de la Base Créée

La migration génère **21 tables** complètes :
- **administrative_divisions** - Hiérarchie administrative
- **regions**, **departments**, **arrondissements**, **communes**
- **voting_centers**, **polling_stations** - Infrastructure électorale  
- **candidates** - Données candidats
- **users**, **role_permissions** - Gestion utilisateurs
- **election_results**, **result_submissions** - Résultats
- **verification_tasks**, **verification_history** - Processus validation
- **hourly_turnout** - Statistiques participation
- **bureau_assignments** - Affectations bureaux

## 🔑 Chaînes de Connexion par Environnement

### Développement
```
Host=localhost;Database=Vc2025_Dev;Username=vcblazor;Password=VcBlazor2025!;Port=5432;
```

### Production  
```
Host=your-prod-server;Database=Vc2025;Username=vcblazor;Password=YourSecurePassword;Port=5432;SSL Mode=Require;
```

## ⚡ Commandes Utiles

```bash
# Créer une nouvelle migration
dotnet ef migrations add NomMigration

# Appliquer les migrations
dotnet ef database update

# Annuler la dernière migration
dotnet ef migrations remove

# Générer un script SQL
dotnet ef migrations script

# Vérifier l'état des migrations
dotnet ef migrations list

# Supprimer la base (attention!)
dotnet ef database drop
```

## 🔍 Vérification Post-Migration

```sql
-- Connectez-vous à la base
psql -h localhost -U vcblazor -d Vc2025_Dev

-- Lister toutes les tables
\dt

-- Vérifier les contraintes
\d+ candidates

-- Exemple de requête
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public';
```

## 🚨 Résolution des Problèmes

### Erreur d'Authentification
- Vérifier que PostgreSQL fonctionne : `pg_ctl status`
- Réinitialiser le mot de passe postgres
- Vérifier pg_hba.conf pour l'authentification

### Port Occupé
- Changer le port dans la chaîne de connexion
- Ou arrêter le service existant

### Permission Refusée  
- Donner les droits sur la base à l'utilisateur
- Vérifier les rôles PostgreSQL

## 💡 Mode Démo
Si vous voulez juste tester l'application rapidement :
```bash
dotnet run
```
L'application démarre automatiquement avec des données factices !