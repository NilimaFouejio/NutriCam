# NutriCam : architecture en couches

```
Views (XAML)  ->  ViewModels  ->  Services  ->  Api (INutriCamApi)  ->  serveur
                                      |               ^
                                      v               |
                                    Data (JSON local) |
                                      ^               |
                       Mapping (ApiMapper) : seul point qui connaît modèles ET contrat
```

| Couche | Dossier | Rôle |
|---|---|---|
| Présentation | `Views/`, `ViewModels/`, `Controls/` | Écrans. `BaseViewModel` fournit `IsBusy`, `ErrorMessage`, `RunAsync`. `ProgramFlow` gère les dialogues (réessayer / hors ligne). |
| Services | `Services/` | `AuthService`, `ProfileService`, `ProgramService`, `CatalogService`, `ProgramStore` (état partagé). Règles : repli hors ligne, idempotence, resynchronisation. |
| API | `Api/` | `INutriCamApi` (une méthode par route), `NutriCamApiClient` (HTTP/JSON), `ApiException` (erreurs typées), `Dtos/`. |
| Mapping | `Mapping/ApiMapper.cs` | Modèles de l'app <-> DTO du contrat. |
| Données | `Data/` | `IAppDataRepository` + fichier JSON, `ICatalogCache`, `ITokenStore` (SecureStorage). |

## Base de données serveur
Identifiants en `VARCHAR(36)` (UUID texte) générés par le serveur. L'application les manipule comme de simples chaînes : aucun changement de code côté mobile.
La table `objectifs` n'a ni date ni poids cible : un seul objectif courant par profil (contrainte `UNIQUE (id_profil)` dans `schema-additions.sql`).

## Persistance locale (1)
- `nutricam-data.json` : compte, profil, préférences, programme, identifiants serveur (`AppData.Remote`). Écriture atomique.
- `nutricam-catalogue.json` : aliments et activités (ETag, rafraîchi au plus 1 fois par jour).
- Jeton : `SecureStorage`, jamais dans les fichiers.
- Remplacer par SQLite = écrire une autre implémentation de `IAppDataRepository`, rien d'autre ne change.

## Contrat (2)
`docs/api/nutricam-openapi.yaml` (OpenAPI 3) + `docs/api/schema-additions.sql` (colonnes à ajouter aux 14 tables).
Équipe API et équipe mobile travaillent en parallèle sur ce seul fichier.
Backend Spring Boot (dossier `nutricam-backend`) :
```bash
cp .env.example .env     # puis changer les mots de passe et le secret JWT
docker compose up --build
```
L'application vise `http://10.0.2.2:8080/api/v1/` (émulateur Android) ou `http://localhost:8080/api/v1/` (Windows, simulateur iOS).
Téléphone réel : renseigner `DevHost` dans `Api/ApiSettings.cs` avec l'adresse IP de l'ordinateur (même Wi-Fi), et autoriser le port 8080 dans le pare-feu.
Autre adresse sans recompiler : `Preferences.Default.Set("nutricam.api.url", "https://…/api/v1/")`.
Sans backend, un serveur factice peut encore répondre à partir du contrat : `npx @stoplight/prism-cli mock docs/api/nutricam-openapi.yaml -p 4010`
(adapter alors le port dans `ApiSettings`).
Avant la production : retirer `usesCleartextTraffic` (Android) et `NSAllowsLocalNetworking` (iOS), et utiliser HTTPS.

## Consommation (3) et chargement
| Action de l'utilisateur | Route | Si le réseau manque |
|---|---|---|
| Créer un compte | POST /auth/inscription | message + nouvelle tentative |
| Se connecter | POST /auth/connexion | connexion locale si le compte est déjà sur l'appareil |
| Générer le programme | PUT /profils/moi, POST /profils/moi/programmes | choix « Réessayer / Générer hors ligne » |
| Marquer un repas mangé | PATCH /programmes/{id}/repas/{id} | enregistré localement, renvoyé plus tard (`Meal.DoneDirty`) |
| Nouvel appareil | GET /profils/moi, GET /programmes/{id}/semaine | |
| Supprimer le compte | DELETE /utilisateurs/moi | refusé (rien n'est supprimé) |

Chargement : `IsBusy` désactive les boutons pendant l'appel (timeout 15 s) ; les erreurs deviennent des `ApiException` avec message français.

## Ce qui reste local, volontairement
- Conditions de santé (jamais envoyées au serveur).
- Remplacement d'un aliment dans un repas (l'API propose des alternatives de recette entière, pas de composant) : `ProgramService.GetAlternativesAsync` est prête, l'écran Alternatives n'y est pas encore branché.
- Plan d'activité affiché (`ActivityPlanner`) : les durées sont exclues du contrat ; `GetRecommandationsActiviteAsync` est prête mais non branchée.
