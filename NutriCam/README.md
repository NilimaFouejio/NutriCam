# NutriCam

Application mobile **.NET MAUI** de génération de programmes nutritionnels personnalisés (contexte camerounais :
riz, plantain, manioc, haricots, poisson…). Les écrans reproduisent la maquette Figma `Nutri.fig` (11 écrans).

> Avancement du dépôt : **étape 5 / 5** (les étapes 1 à 4 reproduisent la maquette ; l’étape 5 apporte les corrections)

## Prérequis

- Visual Studio 2022 (17.12+) ou plus récent avec la charge de travail **Développement d'applications mobiles .NET (MAUI)**,
  ou le SDK .NET 9 + `dotnet workload install maui`.
- Cibles : Android, Windows 10/11, et iOS + Mac Catalyst sur macOS. Voir `INTEGRATION.md` pour l'intégration dans un projet neuf.

## Lancer

```bash
dotnet restore
dotnet build -t:Run -f net9.0-android
```

Ou ouvrir `NutriCam.csproj` dans Visual Studio, choisir l'émulateur Android puis **F5**.

> Autre version de .NET : changer `net9.0-android` / `net9.0-ios` / `net9.0-maccatalyst` / `net9.0-windows10.0.19041.0` dans `NutriCam.csproj`
> (ex. `net8.0-android`) ; `CommunityToolkit.Mvvm` 8.x convient à toutes les versions.

## Parcours utilisateur (11 écrans)

| # | Écran | Route Shell | Fichiers |
|---|-------|-------------|----------|
| 1 | Accueil | `//accueil` | `Views/AccueilPage` |
| 2 | Connexion | `connexion` | `Views/ConnexionPage` |
| 3 | Inscription | `inscription` | `Views/InscriptionPage` |
| 4 | Profil (1/5) | `profil` | `Views/ProfilPage` |
| 5 | Objectif (2/5) | `objectif` | `Views/ObjectifPage` |
| 6 | Préférences (3/5) | `preferences` | `Views/PreferencesPage` |
| 7 | Préférences et budget (4/5) | `budget` | `Views/BudgetPage` |
| 7 bis | **Récapitulatif et activité physique (5/5)** | `recap` | `Views/RecapPage` |
| 8 | Tableau de bord | `//dashboard` | `Views/DashboardPage` |
| 9 | Programme de la semaine | `//programme` | `Views/ProgrammePage` |
| 10 | Détail repas | `repas` | `Views/DetailRepasPage` |
| 11 | Alternatives | `alternatives` | `Views/AlternativesPage` |
| 12 | Profil et réglages (étape 5) | `//reglages` | `Views/ReglagesPage` |
| 13 | Bilan de la semaine (étape 5) | `bilan` | `Views/BilanPage` |

## Architecture

MVVM avec `CommunityToolkit.Mvvm`, navigation Shell, injection de dépendances (`MauiProgram.cs`).

```
Controls/     composants réutilisables (en-tête, champ, pastille, segments, cartes, barre du bas…)
Helpers/      icônes Lucide, palette, validation, utilitaires de texte
Models/       Meal, FoodOption, AppData…
Services/     ProgramStore (état + persistance locale), AuthService, MenuGenerator, FoodCatalog
ViewModels/   un ViewModel par écran + éléments de liste (Items/)
Views/        pages XAML
Resources/    couleurs, styles, polices (Outfit, Inter, Lucide), photos de la maquette
```

## Étapes de développement (un commit + une étiquette Git par étape)

| Étape | Étiquette | Contenu |
|-------|-----------|---------|
| 1 | `etape-1` | Socle du projet (ressources, styles, modèles, services, navigation) + Accueil, Connexion, Inscription |
| 2 | `etape-2` | Parcours « Votre programme » 1/4 à 4/4 : Profil, Objectif, Préférences, Budget + génération |
| 3 | `etape-3` | Tableau de bord, Programme de la semaine, barre de navigation du bas |
| 4 | `etape-4` | Détail d'un repas, Alternatives (remplacement d'un aliment) et boucle complète |
| 5 | `etape-5` | Corrections : schéma d'assiette en français, lisibilité, profil et réglages, suivi des repas et bilan, allergies, avertissement santé |

## Étape 6 : récapitulatif, activité physique, énergie et suivi médical

- **Récapitulatif (5/5)** entre le budget et la génération : profil (sexe compris), activité et repas par jour, objectif, goûts et exclusions, budget et aliments disponibles, santé. Chaque ligne se modifie ; l'étape rouverte revient directement au récapitulatif.
- **Sexe** demandé à l'étape 1 : il sert à estimer le besoin énergétique quotidien (formule de Mifflin-St Jeor, `Services/NutritionCalculator.cs`).
- **Valeur énergétique de chaque plat** (`Meal.Kcal`) : calculée à partir de la composition du repas (légumes, protéines, complément) avec des valeurs moyennes par aliment (`Services/FoodCatalog.cs`). La taille des portions est ajustée chaque jour au besoin de la personne. Elle s'affiche dans les listes, le détail du repas (total et par composante), les alternatives, le tableau de bord (total du jour) et le programme. Elle est recalculée à chaque remplacement d'aliment.
- **Activité physique suggérée par le système** (`Services/ActivityPlanner.cs`) selon l'objectif et le niveau d'activité : nombre de séances, durée, jours et type de séance. L'utilisateur ne choisit pas la fréquence.
- **Suivi médical** : l'étape 1 demande si la personne est concernée par une maladie ou une situation à risque (diabète, hypertension, maladie cardiaque, rénale ou du foie, trouble de l'alimentation, grossesse ou allaitement, autre maladie chronique). Dans ce cas, aucun programme n'est généré (`ProgramStore.IsBlocked`), le programme existant est retiré et l'écran `Views/OrientationPage` invite à consulter un médecin nutritionniste.
- **Prudence énergétique** : pas de réduction des apports pour les moins de 18 ans ni pour un IMC inférieur à 18,5 ; plancher de 1 200 kcal (femme) ou 1 500 kcal (homme).

> Les valeurs énergétiques et le besoin quotidien sont des **estimations** (aliments cuits, moyennes arrondies). Elles ne remplacent pas l'avis d'un professionnel de santé. La liste des situations nécessitant un suivi médical est à faire valider par un professionnel de santé.

## Étape 7 : aucun choix présélectionné, champs de saisie, cases à cocher et défilement

- **Rien n'est coché ni choisi d'avance** : objectif, niveau d'activité, repas par jour, budget, aliments faciles à trouver, sexe et situations de santé sont vides tant que la personne n'a pas répondu (`Goal?`, `Activity?`, `MealsPerDay?`, `Budget?`). Chaque étape exige une réponse avant de continuer.
- **Âge, taille et poids** : champs de saisie classiques, vides au départ (texte d'aide « Votre âge », « Votre taille », « Votre poids »), avec l'unité visible à droite et le clavier numérique.
- **Défilement automatique** : quand on touche « Continuer » alors qu'un élément obligatoire manque, la page défile jusqu'au premier élément à renseigner (`Helpers/ScrollHelper`, événement `ScrollToRequested`). Les champs texte reçoivent directement le curseur de saisie. Écrans concernés : inscription, connexion, profil, objectif et budget.
- **Situations de santé** : cases à cocher (`ViewModels/Items/CheckItem`). « Aucune de ces situations » est exclusive.

## Corrections de l'étape 5

- **Assiette** : la photo aux libellés anglais (FIBERS, MEAT, CARBS) est supprimée ; le schéma ½ / ¼ / ¼ est dessiné dans l'application (`Controls/PlateDiagram`) avec une légende en français.
- **Lisibilité** : plus aucun texte en dessous de 12, texte courant à 14, terracotta (`#9C4529`) et gris (`#6B5A50`) assombris pour atteindre un contraste d'au moins 4,5:1 sur tous les fonds.
- **Profil et réglages** : l'onglet « Profil » ouvre un vrai écran (compte, modification de chaque étape, bilan, déconnexion, suppression du compte et des données).
- **Suivi et bilan** : « Marquer comme mangé » sur le détail d'un repas, bilan jour par jour, et proposition de nouvelle semaine quand les 7 jours sont écoulés.
- **Allergies** : séparées des aliments évités (exclusion stricte) ; un aliment ne peut être que dans une seule des trois listes (aimé, allergie, évité).
- **Avertissement santé** : mention « ne remplace pas un avis médical » sur le profil et dans les réglages.

### Non traité (nécessite des données ou un serveur)

Photos de plats camerounais, calories et grammages, montants en FCFA pour le budget, version anglaise,
vérification de l'e-mail et connexion par téléphone.

## Publier sur GitHub

```bash
git remote add origin https://github.com/<votre-compte>/NutriCam.git
git push -u origin main --tags
```

## Points à connaître

- **Authentification locale** : un compte par appareil, mot de passe haché (PBKDF2). À remplacer par l'API du cahier des charges.
- **Génération locale** : `Services/MenuGenerator.cs` tient compte des aliments évités, des goûts, des aliments disponibles,
  du budget et du nombre de repas. Elle ne calcule pas encore de calories : c'est le point d'entrée pour l'API.
- **Profil** : l'onglet « Profil » relance le parcours 1/4 → 4/4 avec les valeurs déjà saisies.
- Polices : Outfit et Inter (SIL OFL 1.1), icônes Lucide (ISC) — voir `THIRD_PARTY.md`.
