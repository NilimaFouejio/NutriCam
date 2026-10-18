# Intégrer NutriCam dans un projet .NET MAUI neuf (.NET 9)

## 1. Créer le projet de base
```bash
dotnet workload install maui
dotnet new maui -n NutriCam -f net9.0
cd NutriCam
dotnet build -f net9.0-android      # vérifie que l'environnement fonctionne avant de copier quoi que ce soit
```
Visual Studio : *Nouveau projet → Application .NET MAUI*, nom **NutriCam** (le nom doit rester `NutriCam`, c'est le namespace du code).

## 2. Supprimer les fichiers d'exemple du modèle
Dans le projet neuf, supprimer :
`MainPage.xaml`, `MainPage.xaml.cs`, `Resources/Styles/` (Colors.xaml, Styles.xaml), `Resources/Fonts/OpenSans-*.ttf`,
`Resources/Images/dotnet_bot.png`, `Resources/Raw/AboutAssets.txt` (facultatif).

## 3. Copier depuis ce dossier (écraser en cas de conflit)
| Élément | Action |
|---|---|
| `Controls/ Helpers/ Models/ Services/ ViewModels/ Views/` | copier tels quels |
| `Resources/Fonts  Images  Styles  AppIcon  Splash` | copier (remplace l'icône et le splash du modèle) |
| `App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`, `MauiProgram.cs` | remplacer |
| `NutriCam.csproj` | remplacer (cibles Android, iOS, Mac Catalyst, Windows) |
| `Platforms/` | remplacer le dossier entier |
| `.gitignore`, `README.md`, `THIRD_PARTY.md` | copier |

Ne pas copier `bin/`, `obj/`, `.vs/`.

## 4. Restaurer et compiler
```bash
dotnet restore
dotnet build -f net9.0-android
dotnet build -f net9.0-windows10.0.19041.0   # Windows uniquement
dotnet build -f net9.0-ios                   # Mac uniquement
dotnet build -f net9.0-maccatalyst           # Mac uniquement
```
Les cibles iOS et Mac Catalyst n'apparaissent que sur macOS, la cible Windows que sous Windows (voir le `.csproj`).

## 5. Lancer
```bash
dotnet build -t:Run -f net9.0-android
dotnet build -t:Run -f net9.0-windows10.0.19041.0
dotnet build -t:Run -f net9.0-maccatalyst
dotnet build -t:Run -f net9.0-ios -p:_DeviceName=:v2:udid=<UDID>   # simulateur ou appareil
```
Visual Studio : choisir la plateforme dans la barre d'outils puis **F5**.

## 6. Vérifier sur chaque plateforme
1. Accueil → Créer un compte → parcours 1/4 à 4/4 → tableau de bord.
2. Fermer et rouvrir l'application : le compte et le programme sont conservés.
3. Champs de saisie sans soulignement ni cadre parasite (surtout Windows).
4. Programme → détail d'un repas → alternatives → bilan.

## Ce qui a été adapté
- **csproj** : ajout de `net9.0-maccatalyst` et `net9.0-windows10.0.19041.0`, versions minimales de chaque plateforme.
- **Platforms/MacCatalyst** (nouveau) : AppDelegate, Program, Info.plist, Entitlements (sandbox + réseau), manifeste de confidentialité.
- **Platforms/Windows** (nouveau) : App.xaml(.cs), app.manifest, Package.appxmanifest.
- **Platforms/iOS** : `UIRequiresFullScreen` (sans lui, App Store Connect refuse un iPad limité au portrait) + `PrivacyInfo.xcprivacy` (exigé par Apple).
- **MauiProgram.cs** : suppression du soulignement des champs de saisie sur Mac Catalyst et Windows (auparavant Android et iOS seulement).
- **App.xaml.cs** : sur Windows et Mac, fenêtre au format portrait (430×860, largeur max 560) car les écrans sont dessinés pour un téléphone.
- **ProgramStore.cs** : données dans un fichier JSON (`FileSystem.AppDataDirectory`) au lieu de `Preferences`, dont la valeur est limitée à environ 8 Ko sous Windows (un programme de 7 jours la dépasse). Migration automatique des anciennes données.

## Problèmes fréquents
- **`NETSDK1139` / cible non reconnue** : `dotnet workload install maui` ou `dotnet workload update`.
- **Windows, `MSB3073` ou erreur d'empaquetage** : activer le *Mode développeur* (Paramètres → Confidentialité et sécurité → Pour les développeurs).
- **Mac Catalyst, erreur de signature** : dans Visual Studio/Rider, choisir un profil de signature, ou ajouter `<CodesignKey>-</CodesignKey>` pour un test local.
- **iOS, build impossible depuis Windows** : associer un Mac (*Pair to Mac*), Xcode doit être installé et lancé une fois.
- **Polices ou images absentes** : noms en minuscules sans espaces pour les images ; les polices doivent être dans `Resources/Fonts`.
- **Plusieurs projets dans la solution** : `Platforms/` du projet neuf ne doit contenir que les fichiers ci-dessus ; supprimer les doublons `MainApplication`/`App` du modèle.
