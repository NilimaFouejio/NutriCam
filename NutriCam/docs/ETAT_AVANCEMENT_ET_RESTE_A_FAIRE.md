# NutriCam : état d'avancement et reste à faire (document de passation)

Date : 25/10/2026. Projets : `mobile/NutriCam` (.NET MAUI 9) et `backend/nutricam-backend` (Spring Boot 3.3, Java 21, MariaDB, Flyway).
Contrat de référence : `backend/nutricam-backend/nutricam-openapi.yaml` (v1.1.0, repris de `nutricam-openapi-final.yaml`).

## 1. Ce qui a été fait

### Partie 1 : mobile modifié et connecté (fonctions autres que la génération)
**Mobile**
- `Models/Enums.cs` : `ActivityLevel` = 4 profils du tableau 5.2 (TresSedentaire, LegerementActif, ModerementActif, TresActif).
- `Views/ProfilPage.xaml` + `ViewModels/ProfilViewModel.cs` : le choix d'activité est une liste de 4 cartes (`OptionCard`) ; le choix du nombre de repas est supprimé (message « 3 repas par jour »).
- `Models/AppData.cs` : `MealsPerDay` en lecture seule, toujours 3.
- `Mapping/ApiMapper.cs` : valeurs API `tres_sedentaire | legerement_actif | moderement_actif | tres_actif` (anciennes valeurs faible/modere/eleve encore comprises en lecture) ; `repas_par_jour` = 3.
- `ViewModels/RecapViewModel.cs`, `Services/ActivityPlanner.cs` (tableau à 4 colonnes), `Services/NutritionCalculator.cs` (facteurs 1,2 / 1,35 / 1,55 / 1,8) adaptés.
- `Services/ProgramService.cs` : gestion de `CLE_IDEMPOTENCE_REUTILISEE`, commentaire mis à jour.
- `Platforms/Android/AndroidManifest.xml` : `usesCleartextTraffic="true"` (sans lui, l'émulateur ne peut pas joindre `http://10.0.2.2:8080`). À retirer en production.
- `docs/api/nutricam-openapi.yaml` : contrat 1.1.0 avec les 4 niveaux et `repas_par_jour` = 3.
- L'appli était déjà reliée à l'API (auth, profil, catalogues, programme, repas mangé, alternatives) : rien d'autre n'a été touché.

**Backend**
- `dto/Dtos.java` : validation `niveau_activite` (4 valeurs), `repas_par_jour` = 3 (facultatif en entrée).
- `service/ProfilService.java` : `repas_par_jour` toujours enregistré à 3.
- `service/ActiviteService.java` : recommandations d'activité adaptées aux 4 niveaux.
- `V3__niveaux_activite_et_repas_fixes.sql` : conversion des anciennes valeurs, contraintes CHECK.
- Guide de lancement : `GUIDE_LANCEMENT.md`.

### Partie 2 : génération du programme (backend)
- **Moteur de calcul** (Java pur, package `com.nutricam.api.generation`, indépendant de Spring) : Mifflin–St Jeor, facteurs d'activité, cible selon l'objectif, répartition 25/55/20 %, choix des recettes du plan, mise à l'échelle proportionnelle des variantes de recettes, énergie par ligne/repas/jour, exclusions (allergies strictes), contrôles bloquants et avertissements, reproductibilité.
- **Données de référence extraites du Word** (`V5__donnees_reference_plan.sql`, générée par `tools/gen_reference_sql.py` + `tools/parse_plan.py`) : 88 aliments (kcal/g du document), 16 recettes (jus, smoothie, 7 déjeuners, 7 dîners), 45 variantes de quantités (par profil de démonstration), plan de 7 jours.
- **Schéma de traçabilité** (`V4__generation_programme.sql`) : variantes, plan de référence, quantités calculées par repas, cible/paramètres/validation par programme, versions des règles et des données, empreinte de la requête.
- **Intégration Spring** : `GenerationService` (lecture de la référence, enregistrement transactionnel, relecture), `ProgrammeService` réécrit (profil lu côté serveur, Idempotency-Key + empreinte, refus 409 si clé réutilisée avec un autre contenu), `GenerationConfig` + paramètres dans `application.yml`, DTO du contrat 1.1.0 (`synthese_energetique`, `validation`, `ingredients_utilises`, versions).
- **Documentation** : `docs/CATALOGUE_REGLES.md` (règles numérotées, sources, hypothèses), `docs/RAPPORT_ANOMALIES.md` (11 anomalies/décisions).
- **Tests** : 31 tests du moteur (`src/test/java/.../generation`) + `ParcoursDemoTest` complété.

## 2. Ce qui a réellement été exécuté ici
| Élément | Statut |
|---|---|
| Moteur `generation/*.java` | **Compilé** (javac 21) |
| 31 tests du moteur (calculs, 3 profils × 7 jours × 3 repas, règles de repas, allergies, âge/IMC, reproductibilité, anomalies documentées) | **31 réussis, 0 échoué** (voir `RAPPORT_TESTS.md`) |
| Intégration Spring (`GenerationService`, `ProgrammeService`, DTO, config) | **Écrite mais NON compilée** : Maven Central n'est pas joignable depuis l'environnement de travail |
| Migrations V3, V4, V5 | **NON exécutées** sur MariaDB |
| `ParcoursDemoTest` | **NON exécuté** (Docker/Testcontainers indisponibles) |
| Mobile MAUI | **NON compilé** (pas de SDK .NET ici) |

## 3. À vérifier en premier (par l'IA ou le développeur qui reprend)
1. `cd backend/nutricam-backend && mvn -q test` : corriger d'éventuelles erreurs de compilation dans `GenerationService`/`ProgrammeService`/`Dtos` (surtout les surcharges `JdbcTemplate.query` avec lambdas et les records imbriqués).
2. `docker compose up --build` : lire la première erreur Flyway éventuelle (V3 : `DROP CONSTRAINT chk_profils_repas` ; V4 : `ALTER TABLE … ADD COLUMN` multiples et CHECK ; V5 : 100 Ko de INSERT, vérifier les apostrophes).
3. `mvn test` complet avec Docker : `ParcoursDemoTest`.
4. `dotnet build -f net9.0-android` : corriger les erreurs XAML/C# éventuelles de `ProfilPage.xaml`, `ProfilViewModel.cs` (`OptionCard` dans un `BindableLayout`).
5. Parcours réel : inscription → profil (4 niveaux) → récapitulatif → génération → 7 jours affichés.

## 4. Reste à faire
**Décisions métier bloquantes (voir `docs/RAPPORT_ANOMALIES.md`)** : règle officielle de déficit/surplus (H-02, A-07) ; facteur d'activité exact dans chaque fourchette (H-01) ; niveau du profil 3 (A-06) ; cibles de référence 2 500 / 1 868 vs formule (A-03, A-04) ; variantes de recettes par profil (A-08) ; âge minimum et IMC minimum (H-04, H-05) ; limites de mise à l'échelle 0,5–2,0 (H-06) ; liste des ingrédients indirects par allergie (R-20).

**Fonctionnel**
- Recettes de remplacement quand une allergie touche un jour du plan (aujourd'hui : `AUCUN_PROGRAMME_COMPATIBLE`). Exemple : allergie aux œufs ou au poisson = pas de programme. Il faut des recettes validées par jour/repas.
- Alternatives de repas pour les recettes du plan (table `alternatives_repas` vide pour elles).
- Macronutriments (glucides/protéines/lipides 50/20/30 %) : formules dans les Fiches, non demandées par le contrat, non implémentées.
- Budget, aliments préférés et disponibles : enregistrés, non utilisés (aucune règle dans les documents).
- Données nutritionnelles : `proteines_100g`, `glucides_100g`, `lipides_100g` sont NULL pour les aliments du plan ; catégories (`legume`, `proteine`, `feculent`, `autre`) attribuées par mots-clés dans `tools/gen_reference_sql.py`, à relire.
- Aliments « marqueurs d'exclusion » (Arachides, Gluten, Lait, Œufs, Poisson…) ajoutés dans V5 pour que les allergies proposées par l'appli soient enregistrées : à relire.

**Mobile**
- Afficher la cible énergétique, les kcal par repas, `ingredients_utilises` et l'état de validation renvoyés par le serveur (les DTO ne les lisent pas encore ; l'appli ignore les champs inconnus).
- Aligner `NutritionCalculator.DailyTarget` (mode hors ligne) sur le serveur (A-11) ou la retirer.
- Messages d'erreur dédiés pour `ECHEC_VALIDATION_PROGRAMME` et `DONNEES_NUTRITIONNELLES_INCOMPLETES` (aujourd'hui : message du serveur affiché).
- Données locales déjà enregistrées avec l'ancien niveau d'activité (0/1/2) : le nouvel index a un autre sens ; réinstaller l'appli ou se reconnecter pour restaurer le profil du serveur.
- Retirer `usesCleartextTraffic` et passer en HTTPS avant toute diffusion.

**Backend / qualité**
- Exécuter et corriger les tests (§ 3) ; ajouter des tests d'intégration : génération pour les 3 objectifs, 409 `PROFIL_INCOMPLET` sans niveau d'activité, 422 `AUCUN_PROGRAMME_COMPATIBLE` (allergie), lecture après génération.
- Endpoints du prompt non prévus par le contrat 1.1.0 et non créés : `GET` du détail d'un repas, recalcul/validation d'un programme existant, consultation des anomalies.
- Anciens modèles prédéfinis (`modeles_programmes`, `modeles_repas`, V2) : inutilisés par la génération ; à supprimer lors d'une migration ultérieure si le responsable métier le confirme.
- Journalisation technique des générations (sans données personnelles) non ajoutée.

## 5. Outils
`tools/parse_plan.py` : lit le Word (python-docx) → `plan.json`. `tools/gen_reference_sql.py` : produit `V5__donnees_reference_plan.sql` et les TSV de tests. Relancer ces scripts si le document source change (chemin des fichiers à adapter en tête de script).
